<template>
<section class="section bg-light" id="features">
    <div class="container">
        <div class="row justify-content-center mb-5">
            <div class="col-lg-7 text-center">
                <h2 class="fw-bold">{{ t("content.feathrues.title") }}</h2>
                <p class="text-muted">{{ t("content.feathrues.bio") }}</p>
            </div>
        </div>
        <div class="row align-items-center mb-5">
            <div class="col-md-5 order-2 order-md-1 mt-md-0 mt-5">
                <h2 class="mb-4">{{ t("content.feathrues.section[0].title") }}</h2>
                <p class="text-muted mb-5">{{ t("content.feathrues.section[0].description") }}</p>
                <a href="#" class="btn btn-primary">More <i class="icon-xs ms-2" data-feather="arrow-right"></i></a>
            </div>
            <div class="col-md-6 ms-md-auto order-1 order-md-2">
                <div class="position-relative">
                    <div class="ms-5 features-img">
                        <img src="/images/windows_server.jpg" alt="" class="img-fluid d-block mx-auto rounded shadow" />
                    </div>
                    <img src="/images/dot-img.png" alt="" class="dot-img-left" />
                </div>
            </div>
        </div> 
        <div class="row align-items-center section pb-0">
            <div class="col-md-6">
                <div class="position-relative mb-md-0 mb-5">
                    <div class="me-5 features-img" style="background-color:#FFF;">
                        <img src="/images/js.png" alt="" class="img-fluid d-block mx-auto rounded shadow" />
                    </div>
                    <img src="/images/dot-img.png" alt="" class="dot-img-right" />
                </div>
            </div>
            <div class="col-md-5 ms-md-auto">
                <h2 class="mb-4">{{ t("content.feathrues.section[1].title") }}</h2>
                <p class="text-muted mb-5">{{ t("content.feathrues.section[1].description") }}</p>
                <a href="#" class="btn btn-primary">More <i class="icon-xs ms-2" data-feather="arrow-right"></i></a>
            </div>
        </div>
    </div>
</section>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
const { t, locale } = useI18n();
</script>