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
import PowerSystemPsseLibrary as powerlib
import numpy as np
import scipy as sp

psspy.psseinit(80000)
import StringIO

from studydata_IEEE118 import bus_num,response_buses,line_trip,pq,load_bus, load_bus_region, gen_bus_region


# endregion

#======================================================================================================================


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

def change_load(load_bus,percentage):
    psspy.bsys(0,0,[0.0,0.0],0,[],len(load_bus),load_bus,0,[],0,[])
    psspy.scal(sid = 0,all = 0, apiopt = 0,status1 = 2, status3 = 1, status4 = 1, scalval1 = percentage)

def change_gen(gen_bus,increment):
    psspy.bsys(0,0,[0.0,0.0],0,[],len(gen_bus),gen_bus,0,[],0,[])
    psspy.scal(sid = 0,all = 0, apiopt = 0,status1 = 3, scalval2 = increment)

def LoadIncreaseMW(load_bus,percentage):
    psspy.bsys(0,0,[0.0,0.0],0,[],len(load_bus),load_bus,0,[],0,[])
    ierr,allBusLoad = psspy.aloadcplx(0,1,['MVAACT'])
    allBusLoad = allBusLoad[0]
    BusLoadReal = np.real(allBusLoad)
    return np.sum(BusLoadReal)*percentage/100

def product(*args, **kwds):
    # product('ABCD', 'xy') --> Ax Ay Bx By Cx Cy Dx Dy
    # product(range(2), repeat=3) --> 000 001 010 011 100 101 110 111
    pools = map(tuple, args) * kwds.get('repeat', 1)
    result = [[]]
    for pool in pools:
        result = [x+[y] for x in result for y in pool]
    for prod in result:
        yield tuple(prod)

def zerolistmaker(n):
    listofzeros = [0] * n
    return listofzeros

def control_cap(combination):
    for m in range(0,len(combination)):
        if combination[0] == 1:
            psspy.shunt_data(34,r"""1""",1,[_f, 50])
        if combination[1] == 1:
            psspy.shunt_data(44,r"""1""",1,[_f, 50])  
        if combination[2] == 1:
            psspy.shunt_data(45,r"""1""",1,[_f, 50]) 
        if combination[3] == 1:
            psspy.shunt_data(48,r"""1""",1,[_f, 100]) 
        if combination[4] == 1:
            psspy.shunt_data(74,r"""1""",1,[_f, 100])
        if combination[5] == 1:
            psspy.shunt_data(105,r"""1""",1,[_f, 20]) 

output = StringIO.StringIO()

# define switching comminations:
all_combination = list(product([0, 1], repeat=6))

#read the randomized operating points
GeneratedOC = np.genfromtxt('random_operating_conditions.csv', dtype=float, delimiter=',') 

tic = time.clock()

for indexControl in range(0,len(all_combination)):

    databaseOCandLabels = 'database_OC_PostControl_' + str(indexControl)+'.csv'
    if os.path.isfile(databaseOCandLabels):
        os.remove(databaseOCandLabels)

    with silence(output):
        secure = []
        Vol_violation = []
        insecure_index = []
        with open(databaseOCandLabels,'a') as f1:
            for i in range(0,len(GeneratedOC)):
                psspy.case('ieee118bus_divided_basecase.sav')

                #Apply Control
                combination = all_combination[indexControl]
                control_cap(combination)

                #change load operating points
                load_change_region1 = GeneratedOC[i][0]
                load_change_region2 = GeneratedOC[i][1]
                load_change_region3 = GeneratedOC[i][2]

                loadIncrement_region1 = LoadIncreaseMW(load_bus_region[0],load_change_region1)
                loadIncrement_region2 = LoadIncreaseMW(load_bus_region[0],load_change_region2)
                loadIncrement_region3 = LoadIncreaseMW(load_bus_region[0],load_change_region3)
                
                change_load(load_bus_region[0],load_change_region1)
                change_load(load_bus_region[1],load_change_region2)
                change_load(load_bus_region[2],load_change_region3)

                change_gen(gen_bus_region[0],loadIncrement_region1)
                change_gen(gen_bus_region[1],loadIncrement_region2)
                change_gen(gen_bus_region[2],loadIncrement_region3)

                savecase = 'ieee118bus_divided_temp.sav'
                psspy.save(savecase)
               
                # secure or not
                psspy.case(savecase)
                psspy.fnsl()

                # check convergency
                N = psspy.solved()

                [bus_voltage,bus_angle] = powerlib.getMeasurements(response_buses)

                # check voltage violation
                if N == 0:
                    # get bus measurements number
                    [bus_voltage,bus_angle] = powerlib.getMeasurements(response_buses)
                    voltage_violation = 0
                    for vol_index in range(0,len(bus_voltage)):
                        if bus_voltage[vol_index] <= 0.844:
                            voltage_violation = 1
                            violation_index = vol_index
                    if voltage_violation == 1:
                        secure.append(2)
                        S = 2
                        Vol_violation.append(1)
                    else:
                        S = 0
                        secure.append(0)
                        Vol_violation.append(0)
                else:
                    insecure_index.append(i)
                    secure.append(1)
                    S = 1
                    Vol_violation.append(2) 

                # Iterations for searching the secure and insecure operating opints with N-1 contingencies
                db_row = []
                # save load:
                db_row.append(S)
                db_row.append(load_change_region1)
                db_row.append(load_change_region2)
                db_row.append(load_change_region3)

                # save data base raw in csv
                writer = csv.writer(f1,delimiter = ',',lineterminator = '\n')
                writer.writerow(db_row)

                        
    security_ratio=  float(len(secure) - np.count_nonzero(secure))/len(secure)
    voltage_violation_ratio = float(len(secure) - Vol_violation.count(1))/len(secure)
    toc = time.clock() 
    total_time = toc - tic
    print('total time spent = '+str(total_time))
    print'security ratio is'
    print(security_ratio)

    

