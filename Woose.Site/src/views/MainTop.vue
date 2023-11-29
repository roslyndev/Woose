<template>
<section class="hero-1 bg-center bg-primary position-relative" style="background-image: url(/images/hero-1-bg.png);" id="home">
    <div class="container">
        <div class="row align-items-center hero-content">
            <div class="col-lg-5">
                <h1 class="text-white display-4 font-weight-semibold mb-4 hero-1-title">{{ t("content.main.title") }}</h1>
                <p class="text-white-70 mb-4  mb-lg-5">{{ t("content.main.bio") }}</p>
                <a href="https://www.merrytoktok.com" target="_blank" class="btn btn-lg btn-light rounded-pill me-2">{{ t("content.main.button") }}</a>
            </div>
            <div class="col-lg-6 col-sm-10 mx-auto ms-lg-auto me-lg-0">
                <div class="mt-lg-0 mt-4 s800x600">
                    <img src="/images/merrytoktok.png" alt="" class="img-md-responsive" />
                </div>
            </div>
        </div>
    </div>
    <div class="hero-bottom-shape">
        <img src="/images/hero-1-bottom-shape.png" alt="" class="img-fluid d-block mx-auto" />
    </div>
</section>
</template>

<script setup lang="ts">
import config from '@/Config';
import { useI18n } from 'vue-i18n';
const { t, locale } = useI18n();
</script>