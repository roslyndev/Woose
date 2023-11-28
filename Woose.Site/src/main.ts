import { createApp } from 'vue'
import App from './App.vue'
import router from './router'
import store from './store'
import language from './language'; 
//import signalr from './models/signalr';
import { Utility } from './models';

const app = createApp(App);
app.use(store);
app.use(router);
app.use(language);
//app.config.globalProperties.signalr = signalr;
app.config.globalProperties.$filters = {
    formatDate(value:Date, format:string) {
        if (value) {
            return Utility.formatDate(value, format);
        }
    }
};
app.mount('#app');
