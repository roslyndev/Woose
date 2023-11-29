<template>
<section class="section bg-gradient-primary">
    <div class="bg-overlay-img" style="background-image: url(/images/demos.png);"></div>
    <div class="container">
        <div class="row justify-content-center">
            <div class="col-lg-8">
                <div class="text-center">
                    <h1 class="text-white mb-4">{{ t("content.team.title") }}</h1>
                    <p class="text-white mb-5 font-size-16">{{ t("content.team.bio") }}</p>
                    <a href="https://www.facebook.com/rosylndev" target="_blank" class="btn btn-lg btn-light">{{ t("content.team.button") }}</a>
                </div>
            </div>
        </div>
    </div>
</section>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
const { t, locale } = useI18n();
</script>