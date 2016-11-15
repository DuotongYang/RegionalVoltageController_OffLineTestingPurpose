clear all;
close all;
xtrainName = 'xtrain_afterTopologyChange';
xtestName = 'xtest_afterTopologyChange';
ytestName = 'ytest_afterTopologyChange';
ytrainName = 'ytrain_afterTopologyChange';
[dataClass, RatioInsecureOCInStableOC] = CreateDatabaseforTrainingAndTesting(0.6,'databaseMeasurements_topologyChange.csv','database_OC_PostControl_16.csv',xtrainName,ytrainName,xtestName,ytestName);