# region [ Environmental Setup ]
from __future__ import with_statement
from __future__ import division
from contextlib import contextmanager
import os, sys, csv, pdb

PSSE_LOCATION_34 = r"""C:\Program Files (x86)\PTI\PSSE34\PSSPY27"""
PSSE_LOCATION_33 = r"""C:\Program Files (x86)\PTI\PSSE33\PSSBIN"""
if os.path.isdir(PSSE_LOCATION_34):
    sys.path.append(PSSE_LOCATION_34)
    import psse34, psspy

else:
    os.environ['PATH'] = PSSE_LOCATION_33 + ';' + os.environ['PATH']
    sys.path.append(PSSE_LOCATION_33)
    import psspy

from psspy import _i, _f  # importing the default integer and float values used by PSS\E(every API uses them)
from pprint import pprint
import numpy as np
import scipy as sp

psspy.psseinit(80000)
import StringIO

import PowerSystemPsseLibrary1_1 as powerlib
import PythonLibrary as pythoblib


#endregion

# region [ Define SubFunctions ]

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

def control_cap(combination):
    for m in range(0, len(combination)):
        if combination[0] == 1:
            psspy.shunt_data(34, r"""1""", 1, [_f, 50])
        if combination[1] == 1:
            psspy.shunt_data(44, r"""1""", 1, [_f, 50])
        if combination[2] == 1:
            psspy.shunt_data(45, r"""1""", 1, [_f, 50])
        if combination[3] == 1:
            psspy.shunt_data(48, r"""1""", 1, [_f, 100])
        if combination[4] == 1:
            psspy.shunt_data(74, r"""1""", 1, [_f, 100])
        if combination[5] == 1:
            psspy.shunt_data(105, r"""1""", 1, [_f, 20])

# endregion

if __name__ == '__main__':
    # controlCombination = sys.argv[1]
    # control = [int(ctl) for ctl in controlCombination]
    # print control
    ierr = psspy.case("ieee118bus_divided_basecase")
    print ierr
    all_bus = powerlib.findAllBuses()
    # control_cap(control)
    # psspy.fdns()
    # bus_voltage, bus_angle = powerlib.getMeasurements(all_buses)
    # pprint(bus_voltage)









