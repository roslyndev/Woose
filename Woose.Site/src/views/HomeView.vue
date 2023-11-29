<template>
    <Loading v-if="!pageinfo.isloaded" />
    <NavView />
    <MainTop />
    <Services />
    <Features />
    <ContentBannerTop />
    <Members />
    <Blog />
    <ContentBannerBottom />
    <Contactus />
    <Footer />
</template>

<script setup lang="ts">
    import { ref, onMounted } from 'vue';
    import { NavView,MainTop,Services,Features,ContentBannerTop,Members,Blog,ContentBannerBottom,Contactus,Footer,Loading } from '.';

    var pageinfo = ref({
        key:0,
        isloaded:false
    });

    onMounted(() => {
        setTimeout(() => {
            pageinfo.value.isloaded = true;
        }, 1500);
    });
</script>
