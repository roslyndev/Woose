<template>
<footer class="footer" style="background-image: url(/images/footer-bg.png);">
    <div class="container">
        <div class="row">
            <div class="col-lg-4">
                <div class="mb-4">
                    <a href="/"><img src="/images/logo/roslyn.png" alt="" class="rounded-full" height="30" /></a>
                    <p class="text-white-50 my-4">{{ t("global.bio") }}</p>
                </div>
            </div>
            <div class="col-lg-7 ms-lg-auto">
                <div class="row">
                    <div class="col-lg-3 col-6">
                        <div class="mt-4 mt-lg-0">
                            <h4 class="text-white font-size-18 mb-3">Github</h4>
                            <ul class="list-unstyled footer-sub-menu">
                                <li><a href="https://github.com/roslyndev" target="_blank" class="footer-link">Repository</a></li>
                            </ul>
                        </div>
                    </div>
                    <div class="col-lg-3 col-6">
                        <div class="mt-4 mt-lg-0">
                            <h4 class="text-white font-size-18 mb-3">Product</h4>
                            <ul class="list-unstyled footer-sub-menu">
                                <li><a href="https://www.nuget.org/packages/Woose.Data" target="_blank" class="footer-link">Woose</a></li>
                                <li><a href="https://www.merrytoktok.com" target="_blank" class="footer-link">Merrytoktok</a></li>
                            </ul>
                        </div>
                    </div>
                    <div class="col-lg-3 col-6">
                        <div class="mt-4 mt-lg-0">
                            <h4 class="text-white font-size-18 mb-3">Blog</h4>
                            <ul class="list-unstyled footer-sub-menu">
                                <li><a href="https://parkheesung.tistory.com" class="footer-link">{{ t("content.footer.blog[0].title") }}</a></li>
                                <li><a href="https://roslyndev.tistory.com" class="footer-link">{{ t("content.footer.blog[1].title") }}</a></li>
                                <li><a href="https://earningalone.tistory.com" class="footer-link">{{ t("content.footer.blog[2].title") }}</a></li>
                            </ul>
                        </div>
                    </div>
                    <div class="col-lg-3 col-6">
                        <div class="mt-4 mt-lg-0">
                            <h4 class="text-white font-size-18 mb-3">Support</h4>
                            <ul class="list-unstyled footer-sub-menu">
                                <li><a :href="`mailto:${config.info.email}`" class="footer-link">Email</a></li>
                                <li><a href="#contact" class="footer-link">Contact</a></li>
                            </ul>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div class="row">
            <div class="col-lg-12">
                <div class="text-center mt-5">
                    <p class="text-white-50 f-15 mb-0">
                        2023 © Roslyn Development.</p>
                </div>
            </div>
        </div>
    </div>
</footer>
</template>

<script setup lang="ts">
import config from '@/Config';
import { useI18n } from 'vue-i18n';
const { t, locale } = useI18n();
</script>