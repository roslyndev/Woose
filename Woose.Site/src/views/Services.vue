<template>
        <section class="section" id="services">
            <div class="container">
                <div class="row justify-content-center mb-5">
                    <div class="col-lg-7 text-center">
                        <h2 class="fw-bold">{{ t("content.services.title") }}</h2>
                        <p class="text-muted">{{ t("content.services.bio") }}</p>
                    </div>
                </div>
                <div class="row">
                    <div class="col-lg-4" v-for="i in [0,1,2]">
                        <div class="service-box text-center px-4 py-5 position-relative mb-4">
                            <div class="service-box-content p-4">
                                <div class="icon-mono service-icon avatar-md mx-auto mb-4">
                                    <i :class="t(`content.services.section[${i}].icon`)"></i>
                                </div>
                                <h4 class="mb-3 font-size-22">{{ t(`content.services.section[${i}].title`) }}</h4>
                                <p class="text-muted mb-0">{{ t(`content.services.section[${i}].description`) }}</p>
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