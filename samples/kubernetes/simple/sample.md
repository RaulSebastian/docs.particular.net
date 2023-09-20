https://minikube.sigs.k8s.io/docs/handbook/pushing/

eval $(minikube docker-env)


loginServer="$(minikube ip):32772"
dotnet publish -c Release /t:PublishContainer -p ContainerRegistry=$loginServer


kubectl port-forward --namespace kube-system services/registry 32772:80