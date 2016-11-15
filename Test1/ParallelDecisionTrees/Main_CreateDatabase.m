dir = 'C:\Users\Duotong\Google Drive\researches\PMUbasedVArcontrol\DuotongYang\PSSE_simulation\AdaptiveAda\RegionalVoltageControl\Test1\ParallelDecisionTrees\case_';
for index_control = 1:64
    database_OC_PostControl = strcat('database_OC_PostControl_',int2str(index_control - 1),'.csv');
    folderName = strcat(dir,int2str(index_control),'\');
    mkdir(folderName)
    [dataClass, RatioInsecureOCInStableOC] = CreateDatabaseforTrainingAndTesting(0.6,'databaseMeasurements1.csv',database_OC_PostControl,folderName);
end