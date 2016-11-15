# File:"C:\Users\Duotong\Documents\DuotongYang\PSSE_simulation\ICSEG Power Case 1 - IEEE 14 Bus Systems\20150917_simulation.py", generated on THU, SEP 17 2015  10:10, release 32.00.03

import csv, numpy, os.path

#region [Remove The Originial Files]
fname = 'random_operating_conditions.csv'
if os.path.isfile(fname):
    os.remove(fname)

#endregion


#region [Create Operating Conditions]

numberofoperatingpoint = 25000

with open('random_operating_conditions.csv','a') as f1:
    for i in range(0,numberofoperatingpoint):
        load_change_region1 = numpy.random.uniform(0,150) 
        load_change_region2 = numpy.random.uniform(0,270)
        load_change_region3 = numpy.random.uniform(0,150)
        db_row = []
        db_row.append(load_change_region1)
        db_row.append(load_change_region2)
        db_row.append(load_change_region3)
        # save data base raw in csv
        writer = csv.writer(f1,delimiter = ',',lineterminator = '\n')
        writer.writerow(db_row)


#endregion
