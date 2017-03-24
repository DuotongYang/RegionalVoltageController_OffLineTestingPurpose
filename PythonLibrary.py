#-----------------------------------------------------------------------#
# PSSE Dynamic Libray                                                   #
# Author: Duotong Yang                                                  #
# Version 1.1                                                           #
# Modified date: 20170320                                               #
#                                                                       #
#-----------------------------------------------------------------------#

import csv

def writeRowToCsv(csvfileName,data):
    ########
    # example data format is [[1,2,3],[3,4,5],[5,6,7]]
    ########
    with open(csvfileName,'w') as f:
        g = csv.writer(f,delimiter=',', lineterminator='\n')
        for row in data:
            g.writerow(row)


########################
# python run matlab
#######################

def runMatlab(matlabComments):
    import win32com.client
    h = win32com.client.Dispatch('matlab.application')
    h.Execute(matlabComments)
