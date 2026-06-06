---
layout: home
hero:
  name: Uptime Kuma Operator
  text: GitOps-style monitor registration for Kubernetes
  tagline: Sync ingress resources and custom CRDs into Uptime Kuma dashboards, notifications, and monitors.
  image:
    src: /images/logo.png
    alt: Uptime Kuma Operator logo
  actions:
    - theme: brand
      text: Get started
      link: /guide/getting-started
    - theme: alt
      text: Custom resources
      link: /guide/custom-resources
features:
  - title: Ingress-aware monitor sync
    details: Automatically register Kubernetes ingresses as Uptime Kuma items when they are annotated or when global sync is enabled.
  - title: CRD-backed workflows
    details: Manage HttpMonitor, GenericMonitor, Notification, Dashboard, and Convert resources from Kubernetes manifests.
  - title: Works with existing Uptime Kuma deployments
    details: Point the operator at an existing Uptime Kuma instance or deploy one alongside the operator chart.
  - title: Built for GitOps
    details: Reconcile desired monitoring state from manifests and keep the Uptime Kuma dashboard aligned with your cluster.
---

## What this project does

Uptime Kuma Operator reads Kubernetes resources and keeps Uptime Kuma monitors in sync with them. It is designed for teams that want monitoring definitions to live alongside the rest of their cluster configuration instead of being configured manually in the Uptime Kuma UI.

![Uptime Monitoring Items](/images/uptime-monitoring-items.png)

## What you will find in these docs

- A quick start for installing the Helm chart
- A guide to the custom resources the operator watches
- Operational notes for sync behavior, retries, and status fields
- Example manifests you can adapt to your own cluster

## Suggested reading path

1. [Getting started](/guide/getting-started)
2. [Custom resources](/guide/custom-resources)
3. [Operations](/guide/operations)

## Documentation preview

The homepage screenshot below is generated from the VitePress site itself and committed for reference.

![Docs homepage](/images/docs-homepage.png)
