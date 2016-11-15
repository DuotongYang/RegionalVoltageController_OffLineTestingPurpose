clear;
close all;
clc;
load('xtrain.mat');
load('ytrain.mat');
load('ytest.mat');
load('xtest.mat');
ytrain = cellstr(int2str(ytrain));
ytest = cellstr(int2str(ytest));

%% Train the tree using Adaboosting
ytrain_ada = cellfun(@str2double,ytrain);
t_ada_begin = cputime;
[estimateclass,model] = adaboost('train',xtrain,ytrain_ada,30);
t_ada_end = cputime;
t_ada_duration = t_ada_end - t_ada_begin;

%% Train the tree using Normal
t_defaultTree_begin = cputime;
tree_train_matlab = classregtree(xtrain,ytrain);   
t_defaultTree_end = cputime;
t_defaultTree_duration = t_defaultTree_end - t_defaultTree_begin;


%% Compare their accuracy
testclass=adaboost('apply',xtest,model);
testclass = cellstr(int2str(testclass));
Error_Ada = computeTrainingError(ytest,testclass);

Ynew = eval(tree_train_matlab,xtest);
Error_normal = computeTrainingError(ytest,Ynew);

%% print the time consumption and tree stumps

fprintf('\n');
fprintf('Ending updating session.\n');
fprintf('Ada time usage is %d \n',t_ada_duration);
fprintf('default time usage is %d \n',t_defaultTree_duration);

fprintf('\n');
fprintf('error \n');
fprintf('Ada is %d \n',Error_Ada);
fprintf('normal training is %d \n',Error_normal);

save('TreeStumps','model');
