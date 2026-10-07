import React from 'react'
import { createRoot } from 'react-dom/client'
import { FluentProvider, webLightTheme } from '@fluentui/react-components'
import App from './App'
import './index.css'

const innoveraTheme = {
  ...webLightTheme,
  colorBrandBackground: '#b2402f',
  colorBrandBackgroundHover: '#963628',
  colorBrandBackgroundPressed: '#76291f',
  colorBrandForeground1: '#b2402f',
}

createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <FluentProvider theme={innoveraTheme}>
      <App />
    </FluentProvider>
  </React.StrictMode>
)
