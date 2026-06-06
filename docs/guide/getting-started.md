# Getting started

The operator packages its Kubernetes integration as a Helm chart in `helm/uptime-kuma-operator`. The README already describes the core promise: reconcile Kubernetes ingress and custom resources into Uptime Kuma so monitoring definitions can live with the rest of your manifests.

## Install the chart

```bash
helm repo add uptime-kuma-operator https://jasonrve.github.io/uptime-kuma-operator/
helm repo update
helm install my-release uptime-kuma-operator/uptime-kuma-operator
```

If you are installing from a local checkout, point Helm at the chart directory instead:

```bash
helm install my-release ./helm/uptime-kuma-operator
```

## Configure the operator

The chart exposes a small set of defaults that are worth reviewing before you deploy:

- `kuma_load_all_ingress`: when `true`, all ingress resources are loaded, even if you do not annotate them
- `replicaCount`: number of operator replicas
- `uptimekuma.username`: the username used to log into Uptime Kuma
- `uptimekuma.password`: the password used to log into Uptime Kuma

You can override those values with a `values.yaml` file or with `--set` on the Helm command line.

## Understand the sync model

The operator reconciles resources into Uptime Kuma rather than expecting you to manage monitors manually.

- Ingresses can be picked up automatically when `kuma_load_all_ingress` is enabled
- Ingresses can also be explicitly registered with the `app.uptimekuma/name` annotation
- `HttpMonitor` and `GenericMonitor` CRDs model individual monitors
- `Dashboard` CRDs group monitors into public dashboards
- `Notification` CRDs model notification entries
- `Convert` can copy an existing Uptime Kuma monitor definition back into a Kubernetes resource

## Recommended first check

After installation, create one annotated ingress or one simple `HttpMonitor` resource and watch the operator logs. That confirms the control loop can reach both Kubernetes and Uptime Kuma.
