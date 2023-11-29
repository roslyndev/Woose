<template>
<nav class="navbar navbar-expand-lg fixed-top navbar-custom " id="navbar">
    <div class="container">
        <a class="navbar-brand logo" href="#">
            <div class="logo-dark rounded-full" style="width:28px;height:28px;"><img src="/images/logo/roslyn.png" alt="" height="28" /></div><div class="logo-dark logo-text">Rolsyn</div>
            <div class="logo-light rounded-full" style="width:28px;height:28px;"><img src="/images/logo/roslyn.png" alt="" height="28" /></div><div class="logo-light logo-text">Rolsyn</div>
        </a>
        <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarCollapse" aria-controls="navbarCollapse" aria-expanded="false" aria-label="Toggle navigation">
            <i class="" data-feather="menu"></i>
        </button>
        <div class="collapse navbar-collapse" id="navbarCollapse">
            <ul class="navbar-nav ms-auto navbar-center" id="navbar-navlist">
                <li class="nav-item">
                    <a href="#home" class="nav-link">Home</a>
                </li>
                <li class="nav-item">
                    <a href="#services" class="nav-link">Services</a>
                </li>
                <li class="nav-item">
                    <a href="#features" class="nav-link">Features</a>
                </li>
                <li class="nav-item">
                    <a href="#product" class="nav-link">Product</a>
                </li>
                <li class="nav-item">
                    <a href="#blog" class="nav-link">Blog</a>
                </li>
                <li class="nav-item">
                    <a href="#contact" class="nav-link">Contact Us</a>
                </li>
            </ul>
            <a href="https://github.com/roslyndev" target="_blank" class="btn btn-sm rounded-pill nav-btn ms-lg-3">GITHUB</a>
            <select class="language" v-model="selectedLanguage">
                <option v-for="language in languages" :key="language.value" :value="language.value">{{ language.label }}</option>
            </select>
        </div>
    </div>
</nav>
</template>

<script setup lang="ts">
import { reactive,onMounted,ref,watch } from 'vue';
import { useI18n } from 'vue-i18n';

const { t, locale } = useI18n();
var selectedLanguage = ref(localStorage.getItem('selectedLanguage') || 'ko');
const languages = [
    { value: 'en', label: 'English' },
    { value: 'ko', label: '한국어' },
];

watch(selectedLanguage, () => {
  locale.value = selectedLanguage.value;
  localStorage.setItem('selectedLanguage', selectedLanguage.value);
});
</script>