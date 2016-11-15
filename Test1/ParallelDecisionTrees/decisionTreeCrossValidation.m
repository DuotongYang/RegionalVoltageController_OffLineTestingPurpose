function CrossValidationError = decisionTreeCrossValidation(kfold,x,y,index_control)

    Indices = crossvalind('Kfold',length(x), kfold);
    for crossValidationIndex = 1:kfold
        % devide the data into test and train
        [index_test,~] = find(Indices == crossValidationIndex);
        all_index = [1:length(x)];
        index_train = setxor(all_index,index_test);
        xtrain = x(index_train,:);
        ytrain = y(index_train);
        xtest = x(index_test,:);
        ytest = y(index_test);

        % grow tree
        decisionTree = classregtree(xtrain, ytrain);   

        % save tree
        treename = strcat('dt_control_',int2str(index_control),'.mat');
        %save(treename,'decisionTree');

        % compute the training error [Non-crossValidation]
        yPredicted = eval(decisionTree, xtest);
        cm = confusionmat(ytest,yPredicted);  
        N = sum(cm(:));
        trainingError(crossValidationIndex) = ( N-sum(diag(cm))) / N;
    end

        CrossValidationError = sum(trainingError)/10;
end