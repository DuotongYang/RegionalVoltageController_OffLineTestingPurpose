##region [ Environmental Setup ]
from __future__ import with_statement
from __future__ import division
from contextlib import contextmanager
import os,sys, os.path, csv,environment_setup, pdb, time

PSSE_LOCATION_34 = r"""C:\Program Files (x86)\PTI\PSSE34\PSSPY27"""
PSSE_LOCATION_33 = r"""C:\Program Files (x86)\PTI\PSSE33\PSSBIN"""
if os.path.isdir(PSSE_LOCATION_34):
    sys.path.append(PSSE_LOCATION_34)
    import psse34, psspy
else:
    os.environ['PATH'] = PSSE_LOCATION_33 + ';' + os.environ['PATH']
    sys.path.append(PSSE_LOCATION_33)
    import psspy
    
from psspy import _i,_f # importing the default integer and float values used by PSS\E(every API uses them)
MyLibraryLocation = r"""C:\Users\Duotong\Google Drive\researches\PMUbasedVArcontrol\DuotongYang\PSSE_simulation\EMSDataAnalysis\20160908\EMSDataAnalysis\MyLibrary"""
sys.path.append(MyLibraryLocation)
import PowerSystemPsseLibrary as powerlib
import numpy as np
import scipy as sp

psspy.psseinit(80000)
import StringIO


@contextmanager
def silence(file_object=None):
    """
    Discard stdout (i.e. write to null device) or
    optionally write to given file-like object.
    """
    if file_object is None:
        file_object = open(os.devnull, 'w')

    old_stdout = sys.stdout
    try:
        sys.stdout = file_object
        yield
    finally:
        sys.stdout = old_stdout

output = StringIO.StringIO()
with silence(output):

    psspy.psseinit(80000)             # initialize PSS\E in python
    savecase = 'ieee118bus_PSSEcorrected_rq.sav'
    psspy.case(savecase)


    # find all the buses
    psspy.bsys(0,0,[0.0,0.0],1,[1],0,[],0,[],0,[])
    ierr,all_bus = psspy.abusint(0,1,['number'])
    bus_num = all_bus[0]

    #List of all machines
    psspy.bsys(sid = 1,numbus = len(bus_num), buses = bus_num)
    ierr,machine_bus = psspy.amachint(1,1,['NUMBER'])
    machine_bus = machine_bus[0]
    ierr,machine_id =  psspy.amachchar(1,1,['ID'])
    machine_id = machine_id[0]

    #List of all load
    psspy.bsys(sid = 1,numbus = len(bus_num), buses = bus_num)
    ierr,load_bus = psspy.alodbusint(1,1,['NUMBER'])
    load_bus = load_bus[0]
    ierr,load_id =  psspy.aloadchar(1,1,['ID'])
    load_id = load_id[0]

    #List of branches
    ierr,internal_linesfbtb = psspy.abrnint(sid=1,ties=1,flag=1,string=['FROMNUMBER','TONUMBER'])
    ierr,internal_linesid = psspy.abrnchar(sid=1,ties=1,flag=1,string=['ID'])

    #Building the list of contingencies
    line_trip = internal_linesfbtb + internal_linesid  # [[fb1,tb1,id1]]
    response_buses = list(bus_num)

    # export the pq bus
    ierr, bus_type = psspy.abusint(1,1,'type')
    bus_type = bus_type[0]
    pq = []
    for index,bus in enumerate(bus_num):
        if bus_type[index] == 1:
            pq.append(bus)

    # export the slack bus
    slackBus = []
    for index,bus in enumerate(bus_num):
        if bus_type[index] == 3:
            slackBus.append(bus)

    #find the load bus in each region
    region1 = [1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,113,114,115,117]
    region2 = [34,35,36,37,38,39,40,41,42,43,44,45,46,47,48,49,50,51,52,53,54,55,56,57,58,59,60,61,62,63,64,65,66,67,68,69,70,71,72,73,74,75,76,77,78,79,80,81,97,116]
    region3 = [82,83,84,85,86,87,88,89,90,91,92,93,94,95,96,98,99,100,101,102,103,104,105,106,107,108,109,110,111,112,118]
    regions = []
    regions.append(region1)
    regions.append(region2)
    regions.append(region3)

    # check is bus_num and regions are the same. if they are the same, the length would be the length of all buses
    lengthofSameBusNumber = len(list(set(bus_num) & set(region1+region2+region3)))


    # change the owner number based on their regions
    regionSize = 3
    for region_index in range(0,regionSize):
        for bus_index in range(0,len(regions[region_index])):
            psspy.bus_data_2(regions[region_index][bus_index],[_i,_i,_i,region_index + 1])


    # change the load buses owner number based on their regions
    regionSize = 3
    count = 0
    for region_index in range(0,regionSize):
        for bus_index in range(0,len(regions[region_index])):
            #if this bus is a load bus...
            if regions[region_index][bus_index] in load_bus != False:
                psspy.load_data_3(regions[region_index][bus_index],r"""BL""",[_i,_i,_i,region_index + 1,_i],[_f,_f,_f,_f,_f,_f])
                count = count + 1
   
    ###divide the system into 3 parts based on 
    load_bus_region = []
    gen_bus_region = []
    BusLoadRegion = []
    for i in range(1,4):
        psspy.bsys(0,0,[0.0,0.0],0,[],0,[],1,[i],0,[])
        ierr,load_bus_ineachregion = psspy.alodbusint(0,1,['NUMBER'])
        load_bus_ineachregion = load_bus_ineachregion[0]
        load_bus_region.append(load_bus_ineachregion)
        
        ierr,gen_bus_ineachregion = psspy.agenbusint(0,1,['NUMBER'])
        gen_bus_ineachregion =  gen_bus_ineachregion[0]
        gen_bus_region.append(gen_bus_ineachregion)    


    psspy.bsys(0,0,[0.0,0.0],1,[1],0,[],0,[],0,[])
    ierr,bus_voltage = psspy.abusreal(0,1,['PU'])
    bus_voltage = bus_voltage[0]
