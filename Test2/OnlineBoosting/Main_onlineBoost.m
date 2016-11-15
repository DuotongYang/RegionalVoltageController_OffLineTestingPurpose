%% Preparefor Input
clear;
clc;
close all;
load('xtrain.mat')
load('ytrain.mat')
load('ytest.mat')
load('xtest.mat')
load('TreeStumps');

% number of data used to update the trees
NumberofDataforTreeUpdate = 1000;
xlabels = 1:1:NumberofDataforTreeUpdate;

% Randomize the data
indextest = randperm(length(xtest));
xtest = xtest(indextest,:);
ytest = ytest(indextest);
ytrain = cellstr(int2str(ytrain));
ytest = cellstr(int2str(ytest));

% initialize the variables
AdaTimeConsumption = [];
NormalTrainingTimeConsumption = [];
AdaTestError = [];
NormalTestError = [];
index = 0;
SignsChangeCount = [];
ModelTrainingError = [];
featureSelected = [];
Alpha = [];

% number of tree stumps
itt = 30;
newModel = model(1:itt);

% number of data for testing
xtest_modified = xtest(4000+1:end,:);
ytest_modified = ytest(4000+1:end,:);


%% Start Boosting the Tree
for NumberOfNewData = xlabels
    
    
    index = index + 1;
    
    % Get the data for tree update
    xIncremental = xtest(NumberOfNewData,:);
    yIncremental = ytest(NumberOfNewData,:);


    % Add the Incremental data into training data
    dataclass = [ytrain;yIncremental];
    datafeatures = [xtrain;xIncremental];

    
    %% Train the tree using online adaboosting
    yIncremental = cellfun(@str2double,yIncremental);
    
    t_ada_begin = cputime;
    
    [newModel,model,lambda_total] = TreeUpdateOnlineBoosting(newModel,model,xIncremental,yIncremental,datafeatures,itt);
    t_ada_end = cputime;
    t_ada_duration = t_ada_end - t_ada_begin;
    AdaTimeConsumption(index) = t_ada_duration;
    
    Alpha = [Alpha;newModel(:).alpha];
    ModelTrainingError = [ModelTrainingError;newModel(:).error];
    featureSelected = [featureSelected;[newModel(:).key]];
    
    % Compute the online adaboost's test error
    testclass= adaboost('apply',xtest_modified,newModel);
    testclass = cellstr(int2str(testclass));
    Error_Ada = computeTrainingError(ytest_modified,testclass);
    AdaTestError(index) =  Error_Ada;
    
    %% Train the tree using default tree training
    t_defaultTree_begin = cputime;
    tree_train_matlab = classregtree(datafeatures,dataclass);   
    t_defaultTree_end = cputime;
    t_defaultTree_duration = t_defaultTree_end - t_defaultTree_begin;
    NormalTrainingTimeConsumption(index) = t_defaultTree_duration;
    
    % Compute default tree training's test error
    Ynew = eval(tree_train_matlab,xtest_modified);
    Error_normal = computeTrainingError(ytest_modified,Ynew);
    NormalTestError(index) = Error_normal;

end

%% plot time usage
figure;
plot(xlabels,AdaTimeConsumption,'b-');
hold on;
plot(xlabels, NormalTrainingTimeConsumption, 'r-');
legend('Online Boosting Time Consumption','DT Default time consumption');
xlabel('New data number')
ylabel('CPU time/seconds')

%% plot test error
figure;
plot(xlabels,AdaTestError,'b-');
hold on;
plot(xlabels, NormalTestError, 'r-');
legend('Online Boosting Error','DT Default Trianing error');
xlabel('New data number')
ylabel('Test Error');


%% plot training error of tree stumps.
save('ModelTrainingError.mat', 'ModelTrainingError');
figure;
for i = 1:length(ModelTrainingError(1,:))
    plot(ModelTrainingError(:,i));
    hold on
end


