# Operations

This project is a controller, so operational visibility matters. The controllers in `src/Controller` show a common pattern: when dependent services are not ready, the entity is requeued for later reconciliation rather than failing immediately.

## Controller behavior

### IngressController

- reconciles `V1Ingress` resources
- adds an ingress to Uptime Kuma when the operator can reach its services
- removes the monitor when the ingress is deleted

### HttpUptimeEntityController and GenericUptimeEntityController

- reconcile the matching custom resource into Uptime Kuma
- remove the monitor on deletion
- requeue if the operator is not ready yet

### NotificationUptimeEntityController

- creates or deletes notification entries in Uptime Kuma
- requeues while the backing services are unavailable

### DashboardEntityController

- creates dashboard groups from the monitor list in the spec
- updates the CR status field to show whether the dashboard was registered successfully
- retries while the operator is still waiting on dependencies

### ConvertController

- looks up an existing monitor in Uptime Kuma by name
- creates a `GenericMonitor` resource from the live monitor definition
- marks the conversion status as `Converted`

## Status fields you should watch

A few resources expose status fields that are useful during troubleshooting:

- `Dashboard.status.status`
- `Dashboard.status.retries`
- `Notification.status.Status`
- `Convert.status.status`

Those fields make it easier to tell whether the controller is still waiting, already registered, or repeatedly retrying.

## Suggested troubleshooting sequence

1. Check operator logs for service-readiness messages or reconcile failures.
2. Confirm the Uptime Kuma credentials in the Helm values.
3. Verify the CRD YAML matches the resource type you intended.
4. Confirm the ingress annotation or `kuma_load_all_ingress` setting is what you expect.
5. Re-run the reconciliation by editing the resource or deleting/recreating it.

## Why retries matter

The code deliberately requeues resources when services are not ready. That is helpful during startup and while dependencies are restarting, but it also means a broken Uptime Kuma endpoint or invalid credentials can lead to repeated retries. If a resource keeps bouncing, inspect the service connection and the monitor spec first.
