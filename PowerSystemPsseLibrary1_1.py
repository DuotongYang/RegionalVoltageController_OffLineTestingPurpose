#-----------------------------------------------------------------------#
# PSSE Dynamic Libray                                                   #
# Author: Duotong Yang                                                  #
# Version 1.1                                                           #
# Modified date: 20170320                                               #
# Modified: Removed redundant functions
#Change for github
#-----------------------------------------------------------------------#

# Region [Initialization]
from __future__ import with_statement
from __future__ import division
from contextlib import contextmanager
import os,sys, os.path, csv,pdb, time, operator
PSSE_LOCATION_34 = r"""C:\Program Files (x86)\PTI\PSSE34\PSSPY27"""
sys.path.append(PSSE_LOCATION_34)
import psse34, psspy
import random, pdb, time
import numpy
import difflib
import pdb
import scipy
import heapq
import itertools
from scipy import special,optimize
from scipy.sparse import bsr_matrix
from numpy import genfromtxt,max

# endregion

#### Update on OC changing: the region changes becomes owner changes
        
def change_load(load_bus,percentage):
    psspy.bsys(0,0,[0.0,0.0],0,[],len(load_bus),load_bus,0,[],0,[])
    psspy.scal(sid = 0,all = 0, apiopt = 0,status1 = 2, status3 = 1, status4 = 1, scalval1 = percentage)

def change_gen(gen_bus,percentage):
    psspy.bsys(0,0,[0.0,0.0],0,[],len(gen_bus),gen_bus,0,[],0,[])
    psspy.scal(sid = 0,all = 0, apiopt = 0,status1 = 3, scalval2 = percentage)

def LoadIncreaseMW(load_bus,percentage):
    psspy.bsys(0,0,[0.0,0.0],0,[],len(load_bus),load_bus,0,[],0,[])
    ierr,allBusLoad = psspy.aloadcplx(0,1,['MVAACT'])
    allBusLoad = allBusLoad[0]
    BusLoadReal = numpy.real(allBusLoad)
    return numpy.sum(BusLoadReal)*percentage/100


def changeOperatingCondition(numberofRegions,index_OC,loadIncrease,load_bus_region,gen_bus_region):
    #change load operating points
    for region in range(0,numberofRegions):
        
        # Compute load increament in MW   
        loadIncrementMW = LoadIncreaseMW(load_bus_region[region],loadIncrease[index_OC,region])

        # change region load 
        change_load(load_bus_region[region],loadIncrease[index_OC,region])
        
        # re-dispatch Pgen
        change_gen(gen_bus_region[region],loadIncrementMW)

def control_cap(busNumber,MVAR):
    psspy.shunt_data(busNumber,r"""1""",1,[_f, -1*MVAR])


def getMeasurements(buses):
    psspy.bsys(sid = 1,numbus = len(buses), buses = buses)
    ierr,bus_voltage = psspy.abusreal(1,1,['PU'])
    bus_voltage = bus_voltage[0]
    ierr,bus_angle = psspy.abusreal(1,1,['ANGLE'])
    bus_angle = bus_angle[0]
    return bus_voltage,bus_angle

def findFixedShuntBusReal(bus):

    psspy.bsys(sid = 1,numbus = len(bus), buses = bus)
    ierr,bus_fixedshunt = psspy.afxshuntcplx(1,1,['SHUNTACT'])
    bus_fixedshunt = bus_fixedshunt[0]
    bus_fixedshunt = [x*1j for x in bus_fixedshunt]

    return bus_fixedshunt

def findAllBusType(bus_num):
    psspy.bsys(sid=1, numbus=len(bus_num), buses=bus_num)
    ierr,bus_type = psspy.abusint(1,1,'type')
    bus_type = bus_type[0]
    pq = []
    pv = []
    slackBus = []
    for index,bus in enumerate(bus_num):
        if bus_type[index] == 1:
            pq.append(bus)
        elif bus_type[index] == 2:
            pv.append(bus)
        elif bus_type[index] == 3:
            slackBus.append(bus)
    return pq,pv,slackBus

def merge(lsts):
    sets = [set(lst) for lst in lsts if lst]
    merged = 1
    while merged:
        merged = 0
        results = []
        while sets:
            common, rest = sets[0], sets[1:]
            sets = []
            for x in rest:
                if x.isdisjoint(common):
                    sets.append(x)
                else:
                  merged = 1
                  common |= x
            results.append(common)
        sets = results
    return sets


def findAllBuses():
    psspy.bsys(0, 0, [0.0, 0.0], 1, [1], 0, [], 0, [], 0, [])
    ierr, all_bus = psspy.abusint(0, 1, ['number'])
    all_bus= all_bus[0]
    return all_bus

def findAllLoadBuses(bus_num):
    psspy.bsys(sid = 1,numbus = len(bus_num), buses = bus_num)
    ierr,load_bus = psspy.alodbusint(1,1,['NUMBER'])
    load_bus = load_bus[0]
    return load_bus
