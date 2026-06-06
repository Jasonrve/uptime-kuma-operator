# Custom resources

The operator defines a handful of Kubernetes custom resources in the `uptime.kuma/v1` API group.

## HttpMonitor

Use `HttpMonitor` for simple HTTP checks. The entity is intentionally narrow: URL, method, interval, retry cadence, and timeout.

```yaml
apiVersion: uptime.kuma/v1
kind: HttpMonitor
metadata:
  name: example-http-monitor
spec:
  url: https://example.com
  method: GET
  interval: 60
  retryInterval: 60
  resendInterval: 60
  maxretries: 3
  timeout: 48
```

## GenericMonitor

Use `GenericMonitor` when you need the richer Uptime Kuma monitor model. The spec is backed by the operator's `KumaMonitor` model, so it can describe a broader set of monitor types and options.

A minimal example:

```yaml
apiVersion: uptime.kuma/v1
kind: GenericMonitor
metadata:
  name: example-generic-monitor
spec:
  type: http
  name: Example monitor
  url: https://example.com
  method: GET
  interval: 60
  retryInterval: 60
  resendInterval: 60
  maxretries: 0
  timeout: 48
```

## Notification

`Notification` represents notification credentials and delivery settings in Uptime Kuma.

```yaml
apiVersion: uptime.kuma/v1
kind: Notification
metadata:
  name: example-notification
spec:
  type: slack
  name: Slack notifications
  webhookURL: https://hooks.slack.com/services/...
  isDefault: false
  webhookContentType: json
  applyExisting: false
```

## Dashboard

`Dashboard` groups monitors into named sections. The operator maps the monitor names from the dashboard spec into Uptime Kuma public groups.

```yaml
apiVersion: uptime.kuma/v1
kind: Dashboard
metadata:
  name: example-dashboard
spec:
  name: example-dashboard
  description: Example dashboard grouped from multiple monitors
  group:
    - name: API services
      monitorList:
        - example-http-monitor
        - example-generic-monitor
      weight: 0
```

## Convert

`Convert` is a helper resource that copies an existing Uptime Kuma monitor back into Kubernetes form.

```yaml
apiVersion: uptime.kuma/v1
kind: Convert
metadata:
  name: example-convert
spec:
  name: example-monitor-name
```

## Ingress reconciliation

The operator also watches `Ingress` resources directly. Ingresses are reconciled when they are annotated with `app.uptimekuma/name`, and they can also be loaded globally when `kuma_load_all_ingress` is enabled.

## Practical note

The controller code retries when Uptime Kuma or its supporting services are not ready yet. That means it is normal to see requeues and status updates before the operator settles into a steady state.
