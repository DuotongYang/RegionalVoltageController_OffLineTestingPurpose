clear;
close all;
clc;

dir = 'C:\Users\Duotong\Google Drive\researches\PMUbasedVArcontrol\DuotongYang\PSSE_simulation\AdaptiveAda\RegionalVoltageControl\Test1\ParallelDecisionTrees\case_';

for index_control = 1:64;
    folderName = strcat(dir,int2str(index_control),'\');

    xtrainName = strcat(folderName,'xtrain');
    ytrainName = strcat(folderName,'ytrain');
    xtestName = strcat(folderName,'xtest');
    ytestName = strcat(folderName,'ytest');

    load(xtrainName);
    load(ytrainName);
    load(xtestName);
    load(ytestName);
    ytrain = cellstr(int2str(ytrain));
    ytest = cellstr(int2str(ytest));

    tree_train_matlab = classregtree(xtrain,ytrain);

    % 10 folds cross-Validation
    kfold = 10;
    CrossValidationError(index_control) = decisionTreeCrossValidation(kfold,xtrain,ytrain,index_control);
end

figure;
bar(CrossValidationError);
axis([0,64,0.014,0.019])
ylabel('Cross Validation Error Rate');
xlabel('Control Combinations')