function [dataClass, RatioInsecureOCInStableOC] = CreateDatabaseforTrainingAndTesting(percentageForTraining,measurementdata,operatingConditionData,folderName)
    
    MeasurementData = csvread(measurementdata);
    [dataClass,RatioInsecureOCInStableOC] = LabelInsecureOCusing(operatingConditionData);

    %If the post-operating condition from secure becomes unstable, this operating condition will be labeled as Insecure instead of unstable 
    stableBecomesUnstable = 0;
    for indexMeasurement = 1:length(MeasurementData)
        if dataClass(indexMeasurement,1) == 1 && MeasurementData(indexMeasurement,1) ~= 1
            dataClass(indexMeasurement,1) = 4;
            stableBecomesUnstable = stableBecomesUnstable + 1;
        end
    end

    % Get all the stable measurements
    StableDataClass = dataClass(dataClass(:,1) ~= 1, 1);
    StableDataFeature = MeasurementData(dataClass(:,1) ~= 1,2:end);

    % change mode between: 'Detect Insecure Only' or 'Detect LowVoltage Only'
    % For example, if it is 'Detect Insecure Only', all the insecure OC will
    % become -1 and all the stable OC including lowvoltage OC will become 1.
    StableDataClass = TargetIsInsecureOrLowVoltage(StableDataClass,'Detect Insecure Only');

    lastIndexForTraining = round(length(StableDataClass)*percentageForTraining);
    xtrain = StableDataFeature(1:lastIndexForTraining,:);
    ytrain = StableDataClass(1:lastIndexForTraining);
    xtest = StableDataFeature(lastIndexForTraining+1:end,:);
    ytest = StableDataClass(lastIndexForTraining+1:end);
    
    xtrainName = strcat(folderName,'xtrain');
    ytrainName = strcat(folderName,'ytrain');
    xtestName = strcat(folderName,'xtest');
    ytestName = strcat(folderName,'ytest');
    
    save(xtrainName,'xtrain');
    save(ytrainName,'ytrain');
    save(xtestName,'xtest');
    save(ytestName,'ytest');
    
    
function StableDataClass = TargetIsInsecureOrLowVoltage(StableDataClass,mode)
switch(mode)
    case('Detect Insecure Only')
        for indexOC = 1:length(StableDataClass)
            if StableDataClass(indexOC) ~= 4
                StableDataClass(indexOC) = 1;
            else
                StableDataClass(indexOC) = -1;
            end
        end

    
    case('Detect LowVoltage Only')
        for indexOC = 1:length(StableDataClass)
            if StableDataClass(indexOC) == 0
                StableDataClass(indexOC) = 1;
            else
                StableDataClass(indexOC) = -1;
            end
        end
end

function [database,RatioInsecureOCInStableOC] = LabelInsecureOCusing(csvcasename)
database = csvread(csvcasename);
% seperate data into stbale and unstable OC

k = 1;
l = 1; 
for i = 1:length(database)
    if database(i,1) == 1
        unstable(k,:) = database(i,2:4);
        unstableIndex(k) = i;
        k = k + 1;
    else
        stable(l,:) = database(i,2:4);
        stableIndex(l) = i;
        l = l + 1;
    end
end

% find the maximum Euclidean distance from stable OC to unstable OC
distance = 15;
count = 1;
for k = 1:length(stableIndex)
    for l = 1:length(unstableIndex)
        dis(l) = sqeuclidean(stable(k,:),unstable(l,:));
    end
    min_dis = min(dis);
    if  min_dis <= distance
        database(stableIndex(k),1) = 4;
        count = count + 1;
    end
end
RatioInsecureOCInStableOC = count/length(stableIndex);

 function dis = sqeuclidean(a,b)
    dis = norm(a - b);