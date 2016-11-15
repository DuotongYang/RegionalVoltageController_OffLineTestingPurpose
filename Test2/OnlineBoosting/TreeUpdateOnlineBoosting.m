function [newModel,updatedModel,lambda_total] = TreeUpdateOnlineBoosting(newModel,model,datafeatures,dataclass,totaldatafeatue,itt)

% Weight of training samples, for first weight, every sample is even important
% (same weight)
lambda = 1;
NewDataStatus = zeros(1,itt);
updatedModel = model;
lambda_total = [];

boundary=[min(totaldatafeatue,[],1) max(totaldatafeatue,[],1)];

for n = 1:itt

    for t=1:length(model(:))

        % Find the best treshold to seperate the data in two classes\
        h.dimension = model(t).dimension; 
        h.threshold = model(t).threshold;          
        h.direction = model(t).direction;
        model(t).boundary = boundary;

        % compute the estimation error for tree stump
        estimateclass = ApplyClassTreshold(h,datafeatures);
        if estimateclass == dataclass
            model(t).lambdaCorrect = model(t).lambdaCorrect + lambda;
            NewDataStatus(t) = 0;
        else
            model(t).lambdaWrong = model(t).lambdaWrong + lambda;
            NewDataStatus(t) = 1;
        end
        
        model(t).error = model(t).lambdaWrong/(model(t).lambdaCorrect + model(t).lambdaWrong);
        
    end
    
    [~,indexSmallestError] = min([model(:).error]);
    %[~,indexListErrorWorseThanGuess] = find([model(:).error > 0.5]);
    model(indexSmallestError).alpha = 1/2 * log((1-model(indexSmallestError).error)/model(indexSmallestError).error);
    
    % Select the feature
    newModel(n) = model(indexSmallestError);
    
    % Update the original model without deleting any features
    for indexModel = 1:length(model(:))
        if model(indexModel).key == model(indexSmallestError).key
            updatedModel(model(indexSmallestError).key) = model(indexModel);
        end
    end
  
    
    if NewDataStatus(indexSmallestError)== 0
        lambda = lambda/(2*(1-newModel(n).error));
    else
        lambda = lambda/(2*newModel(n).error);
    end
    
    lambda_total = [lambda_total,lambda];
    
    % remove the selected feature from the candidates
    model(indexSmallestError) = [];
    
end
    
