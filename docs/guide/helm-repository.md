# Helm repository

The same GitHub Pages site can serve both the VitePress documentation and the Helm chart repository.

## Repository URL

Use the project Pages URL as your Helm repo:

```bash
helm repo add uptime-kuma-operator https://jasonrve.github.io/uptime-kuma-operator/
helm repo update
helm search repo uptime-kuma-operator
```

## Install the chart

```bash
helm install my-release uptime-kuma-operator/uptime-kuma-operator
```

If you want to install directly from a cloned checkout, use the chart directory instead:

```bash
helm install my-release ./helm/uptime-kuma-operator
```

## What gets published

The Pages build copies the chart index and packages into the published site so Helm can fetch them from the same domain as the docs:

- `index.yaml` at the repository root
- `uptime-kuma-operator-*.tgz` chart packages at the repository root

That means the docs site remains readable in a browser while Helm still sees a normal chart repository.

## Notes

- The chart index is already stored in `docs/index.yaml`
- The packaged chart archives are already stored in `docs/*.tgz`
- The build step copies those files into `docs/.vitepress/dist/` before GitHub Pages deployment
