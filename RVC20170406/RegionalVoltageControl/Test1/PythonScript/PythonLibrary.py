#-----------------------------------------------------------------------#
# PSSE Dynamic Libray                                                   #
# Author: Duotong Yang                                                  #
# Version 1.1                                                           #
# Modified date: 20170320                                               #
#                                                                       #
#-----------------------------------------------------------------------#

import csv,difflib,os,operator

def writeRowToCsv(csvfileName,data):
    ########
    # example data format is [[1,2,3],[3,4,5],[5,6,7]]
    ########
    with open(csvfileName,'w') as f:
        g = csv.writer(f,delimiter=',', lineterminator='\n')
        for row in data:
            g.writerow(row)

def readCSVToRow(csvfileName):
    ########
    # example data format is [[1,2,3],[3,4,5],[5,6,7]]
    ########
    data = []
    f = open(csvfileName)
    csv_f = csv.reader(f)
    for row in csv_f:
        data.append(row)
    f.close()
    return data

def WriteDicToCsv(csvfileName,mydict):
    with open(csvfileName, 'wb') as csv_file:
        writer = csv.writer(csv_file)
        for key, value in mydict.items():
            writer.writerow([key, value])


def ReadCSVToDic(csvfileName):
    with open(csvfileName, 'rb') as csv_file:
        reader = csv.reader(csv_file)
        mydict = dict(reader)
    return mydict

def isFileExist(filename):
    if os.path.isfile(filename):
        return True

def MergeKeysWithSameValue(dic):
    dic2 = {}
    for key in dic.keys():
        if dic2.has_key(dic[key]):
            dic2[dic[key]].append(key)
        else:
            dic2[dic[key]] = [key]
    return dic2

def ComputeSimilarityTwoLists(s1,s2):
    sm = difflib.SequenceMatcher(None,s1,s2)
    return sm.ratio()

def NormalizeList(data):
    dataNormalized = [operator.truediv((float(i) - min(data)),(max(data) - min(data))) for i in data]
    return dataNormalized

########################
# python run matlab
#######################

def runMatlab(matlabComments):
    import win32com.client
    h = win32com.client.Dispatch('matlab.application')
    h.Execute(matlabComments)
