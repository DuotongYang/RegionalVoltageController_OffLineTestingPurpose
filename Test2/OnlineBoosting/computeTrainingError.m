function [trainingError] = computeTrainingError(Ytest, YPredicted)
cm = confusionmat(Ytest,YPredicted);  
N = sum(cm(:));
trainingError = ( N-sum(diag(cm))) / N;