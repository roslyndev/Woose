<template>
<section class="section bg-center w-100 bg-light" style="background-image: url(/images/cta-bg.png);">
    <div class="container">
        <div class="row">
            <div class="col-lg-12">
                <div class="card bg-gradient-primary text-center border-0">
                    <div class="bg-overlay-img" style="background-image: url(/images/demos.png);"></div>
                    <div class="card-body mx-auto p-sm-5 p-4">
                        <div class="row justify-content-center">
                            <div class="col-lg-10">
                                <div class="p-3">
                                    <h2 class="text-white mb-4">{{ t("content.banner.title") }}</h2>
                                    <p class="text-white-70 font-size-16 mb-4 pb-3">{{ t("content.banner.bio") }}</p>
                                    <a href="https://www.merrytoktok.com" target="_blank" class="btn btn-light rounded-pill">{{ t("content.banner.button") }}</a>
                                </div>
                            </div>
                        </div>
                    </div>
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