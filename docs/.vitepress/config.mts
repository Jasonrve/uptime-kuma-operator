import { defineConfig } from 'vitepress'

export default defineConfig({
  lang: 'en-US',
  title: 'Uptime Kuma Operator',
  description: 'Documentation for the Kubernetes operator that synchronizes Uptime Kuma monitors from Kubernetes resources.',
  lastUpdated: true,
  cleanUrls: true,
  themeConfig: {
    logo: '/images/logo.png',
    nav: [
      { text: 'Guide', link: '/guide/getting-started' },
      { text: 'Custom Resources', link: '/guide/custom-resources' },
      { text: 'Operations', link: '/guide/operations' },
      { text: 'GitHub', link: 'https://github.com/Jasonrve/uptime-kuma-operator' }
    ],
    sidebar: {
      '/guide/': [
        {
          text: 'Guide',
          items: [
            { text: 'Getting started', link: '/guide/getting-started' },
            { text: 'Custom resources', link: '/guide/custom-resources' },
            { text: 'Operations', link: '/guide/operations' }
          ]
        }
      ]
    },
    socialLinks: [{ icon: 'github', link: 'https://github.com/Jasonrve/uptime-kuma-operator' }],
    footer: {
      message: 'Documentation for a work-in-progress operator.',
      copyright: 'Copyright © 2026 Jasonrve'
    }
  }
})
