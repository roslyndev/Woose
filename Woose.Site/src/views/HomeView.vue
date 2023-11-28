<template>
    <Loading v-if="!pageinfo.isloaded" />
    <NavView />
    <Section1 />
    <Section2 />
    <Section3 />
    <Section4 />
    <Section6 />
    <Section7 />
    <Section8 />
    <Section9 />
    <Footer />
</template>

<script setup lang="ts">
    import { ref, onMounted } from 'vue';
    import { NavView,Section1,Section2,Section3,Section4,Section5,Section6,Section7,Section8,Section9,Footer,Loading } from '.';

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
