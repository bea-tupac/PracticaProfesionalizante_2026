import './assets/css/base/main.css'
import { createApp } from 'vue'
import { createPinia } from 'pinia'
import ElementPlus from 'element-plus'
import 'element-plus/dist/index.css'
import * as ElementPlusIconsVue from '@element-plus/icons-vue'

import App from './App.vue'
import router from './router'
import { setRouter } from '@/composables/useApiFetch'

const app = createApp(App)

app.use(createPinia())
app.use(router)
app.use(ElementPlus)

// Inyectar el router en useApiFetch para manejo de 401/403
setRouter(router)

for (const [key, component] of Object.entries(ElementPlusIconsVue)) {
  app.component(key, component)
}

app.mount('#app')