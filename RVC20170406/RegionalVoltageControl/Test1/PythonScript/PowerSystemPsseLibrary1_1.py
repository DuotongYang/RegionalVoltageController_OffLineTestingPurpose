#-----------------------------------------------------------------------#
# PSSE Dynamic Libray                                                   #
# Author: Duotong Yang                                                  #
# Version 1.1                                                           #
# Modified date: 20170322                                               #
# Modified: QV Analysis added                                           #
#-----------------------------------------------------------------------#

# Region [Initialization]
from __future__ import with_statement
from __future__ import division
from contextlib import contextmanager
import os,sys, os.path, csv,pdb, time, operator
PSSE_LOCATION_34 = r"""C:\Program Files (x86)\PTI\PSSE34\PSSPY27"""
sys.path.append(PSSE_LOCATION_34)
import psse34, psspy,random, pdb, time,difflib,pdb,scipy,heapq,itertools 
import numpy as np
from scipy import special,optimize
from scipy.sparse import bsr_matrix
from numpy import genfromtxt,max
import matplotlib.pyplot as plt

# endregion

#region
##################################
#  Find System Information
##################################
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

def findBusesArea(buses):
    psspy.bsys(sid=1, numbus=len(buses), buses=buses)
    ierr,buses_area = psspy.abusint(1, 1, ['AREA'])
    buses_area = buses_area[0]
    return buses_area

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

def findFixedShunt(bus):

    psspy.bsys(sid = 1,numbus = len(bus), buses = bus)
    ierr,bus_fixedshunt = psspy.afxshuntcplx(1,1,['SHUNTACT'])
    bus_fixedshunt = bus_fixedshunt[0]
    bus_fixedshunt = [x*1j for x in bus_fixedshunt]

    ierr,busNumber = psspy.afxshuntint(1,1,['NUMBER'])
    busNumber = busNumber[0]

    FixedShunt = {}
    for i in range(0,len(busNumber)):
        FixedShunt[busNumber[i]] = bus_fixedshunt[i]

    return FixedShunt

def getGenReactivePowerOutput(busNumber):
    psspy.bsys(sid=0, numbus=len(busNumber), buses=busNumber)
    ierr,reactivePowerOutput = psspy.agenbusreal(0,1,'QGEN')
    return reactivePowerOutput[0]

def getGenReactivePowerMax(busNumber):
    psspy.bsys(sid=0, numbus=len(busNumber), buses=busNumber)
    ierr,reactivePowerMax = psspy.agenbusreal(0,1,'QMAX')
    return reactivePowerMax[0]

def getGenReactivePowerMin(busNumber):
    psspy.bsys(sid=0, numbus=len(busNumber), buses=busNumber)
    ierr,reactivePowerMin = psspy.agenbusreal(0,1,'QMIN')
    return reactivePowerMin[0]

def busesConnectedToThisBus(startbus):
    buslist = []
    ierr = psspy.inibrn(startbus, 2)
    iteration = 0
    if ierr < 1:
        while iteration < 100:
            ierr, foundbus, ickt = psspy.nxtbrn(startbus)
            if ierr < 1:
                buslist.append(foundbus)
                buslist = list(set(buslist))
            else:
                break
            iteration += 1
    return buslist

# endregion


#############################
#region [Get the System Measurement]
#############################
def getMeasurements(buses):
    psspy.bsys(sid = 1,numbus = len(buses), buses = buses)
    ierr,bus_voltage = psspy.abusreal(1,1,['PU'])
    bus_voltage = bus_voltage[0]
    ierr,bus_angle = psspy.abusreal(1,1,['ANGLE'])
    bus_angle = bus_angle[0]
    return bus_voltage,bus_angle

#endregion

#############################
#region [ Change System Data ]
#############################

def changeOwnerNumber(clusters):
    """
        clusters is a dictionary
        Changes the bus owner number based on clusters' key
    """
    area_num = 2 # 1 is the default area_num. area_num starts from 2
    for key in clusters.keys():
        for bus in clusters[key]:
            ierr = psspy.bus_data_3(bus, intgar2 = area_num)
        area_num += 1

#endregion


#############################
#region [Control the system]
#############################
def change_load(load_bus,percentage):
    psspy.bsys(0,0,[0.0,0.0],0,[],len(load_bus),load_bus,0,[],0,[])
    psspy.scal(sid = 0,all = 0, apiopt = 0,status1 = 2, status3 = 1, status4 = 1, scalval1 = percentage)

def change_gen(gen_bus,percentage):
    psspy.bsys(0,0,[0.0,0.0],0,[],len(gen_bus),gen_bus,0,[],0,[])
    psspy.scal(sid = 0,all = 0, apiopt = 0,status1 = 2, scalval2 = percentage)

def LoadIncreaseMW(load_bus,percentage):
    psspy.bsys(0,0,[0.0,0.0],0,[],len(load_bus),load_bus,0,[],0,[])
    ierr,allBusLoad = psspy.aloadcplx(0,1,['MVAACT'])
    allBusLoad = allBusLoad[0]
    BusLoadReal = numpy.real(allBusLoad)
    return numpy.sum(BusLoadReal)*percentage/100

def control_cap(busNumber,MVAR):
    psspy.shunt_data(busNumber,"1",1,[_f, -1*MVAR])

#endregion

#region [Analysis Functions]
def QVAnalysis(CASE,ireg,activateplot):

    busno = 44999 # Fictitious generator bus
    genid = 1
    status = 1
    pgen = 0.0 # Fict gen P output
    Qlimit = 9999.0 # Fict. gen Q limit
    pmax = 0.0 # Fict gen P limit
    
    #--------------------------------

    def add_machine():
        psspy.plant_data(busno, intgar1=ireg)
        psspy.machine_data_2(
            busno,
            str(genid),
            intgar1=int(status),
            realar1=pgen,
            realar3=Qlimit,
            realar4=-Qlimit,
            realar5=pmax)

    def get_mvar(i):
        """
        Changes the voltage set point at the synchronous machine
        solves the case
        returns the the new reactive power output of the sync machine.
        """
        psspy.plant_data(busno, realar1=i)
        ierr = psspy.fnsl()
        val = psspy.solved()
        if val == 0:
            ierr, mvar = psspy.macdat(busno, str(genid), 'Q')
            return mvar
        else:
            return None
        
    def get_genExhausted(pv):
        """
        get the number of gen whose reactive power got exhausted.
        """
        genExhausted = []
        GenReactivePowerOutput = getGenReactivePowerOutput(pv)
        GenReactivePowerMax = getGenReactivePowerMax(pv)
        GenReactivePowerMin = getGenReactivePowerMin(pv)
        for i in range(0,len(pv)):
            if GenReactivePowerOutput[i] == GenReactivePowerMax[i] \
               or GenReactivePowerOutput[i] == GenReactivePowerMin[i]:
                genExhausted.append(pv[i])
        return genExhausted
        
    
    psspy.psseinit(12000)
    psspy.case(CASE)
    psspy.solution_parameters_3(intgar2=60) # set number of solution iterations.

    psspy.bus_data_2(busno, intgar1=2, name='TEST')
    psspy.branch_data(i=busno, j=ireg)
    all_bus = findAllBuses()
    pq,pv,slackBus = findAllBusType(all_bus)
    add_machine()

    
    # get gen that exhausted its reactive power    
    genExhausted_old = get_genExhausted(pv) 

    pu = [x for x in np.arange(1.0, 0.2, -0.005)]
    varlist = []
    voltagelist = []
    
    for v in pu:
        res = get_mvar(v)
        if res:
            psspy.save("temp")
            varlist.append(res)
            voltagelist.append(v)
        else:
            break

    # get new gen that exhausted its reactive power
    psspy.case("temp")
    busGenExhausted = []
    genExhausted_new = get_genExhausted(pv)
    for bus in genExhausted_new:
        if bus not in genExhausted_old:
            busGenExhausted.append(bus)
                
    QminIndex = np.argmin(varlist)
    Qmin = varlist[QminIndex]
    Vmin = voltagelist[QminIndex]

    if activateplot == 1:
        plt.plot(voltagelist, varlist, '-o')
        plt.plot(Vmin,Qmin,'ro')
        plt.xlabel('PU')
        plt.ylabel('MVar')
        plt.grid()

    return Qmin,Vmin,busGenExhausted


def reduceModel(zonebuses):
    # Reduce the model
    psspy.bsys(sid=1, numbus=len(zonebuses), buses=zonebuses)
    psspy.gnet(sid=1, all=0)
    psspy.island()
    psspy.fdns(options=[1, 0, 1, 1, 1, 0, 0, 0])
    psspy.eeqv(sid=1, all=0, status=[0, 0, 0, 0, 1, 0], dval1=0)
    psspy.island()
    ierr = psspy.fdns(options=[1, 0, 1, 1, 1, 0, 0, 0])
    ival = psspy.solved()
    reducedModelName = "reduced"
    print "ival equal to "
    print ival
    if ival == 0:
        psspy.save(reducedModelName)
    else:
        return None
    return reducedModelName



#endregion
