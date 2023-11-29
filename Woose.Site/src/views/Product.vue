<template>
<section class="section bg-light" id="product" v-if="data.products.length > 0">
    <div class="container">
        <div class="row justify-content-center mb-4">
            <div class="col-lg-7 text-center">
                <h2 class="fw-bold">{{ t("content.product.title") }}</h2>
                <p class="text-muted">{{ t("content.product.bio") }}</p>
            </div>
        </div>
        <div class="row" :key="data.key">
            <div class="col-lg-3 col-sm-6" v-for="(product, index) in data.products">
                <div class="team-box mt-4 position-relative overflow-hidden rounded text-center shadow">
                    <div class="position-relative overflow-hidden">
                        <div class="img-card">
                            <img :src="product.photo" alt="" class="img-fluid d-block mx-auto" :class="product.class"  />
                        </div>
                        <ul class="list-inline p-3 mb-0 team-social-item">
                            <li class="list-inline-item mx-3">
                                <a :href="product.url" class="team-social-icon h-primary" target="_blank"><i class="fa-solid fa-share-from-square"></i></a>
                            </li>
                        </ul>
                    </div>
                    <div class="p-4">
                        <h5 class="font-size-19 mb-1">{{ product.name }}</h5>
                        <p class="text-muted text-uppercase font-size-14 mb-0">{{ product.position }}</p>
                    </div>
                </div>
            </div>
            <div class="col-lg-3 col-sm-6" v-for="(i, j) in data.empty" v-if="data.empty.length > 0">
                <div class="team-box mt-4 position-relative overflow-hidden rounded text-center shadow">
                    <div class="position-relative overflow-hidden">
                        <div class="img-fluid d-block mx-auto empty-card">
                            <i class="fa-solid fa-circle-exclamation"></i>
                        </div>
                    </div>
                    <div class="p-4">
                        <h5 class="font-size-19 mb-1">Empty</h5>
                        <p class="text-muted text-uppercase font-size-14 mb-0">Comming soon</p>
                    </div>
                </div>
            </div>
        </div>
    </div>
</section>
</template>

<script setup lang="ts">
import { ref,onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
const { t, locale } = useI18n();

interface Product {
    photo : string,
    name : string,
    position : string,
    url : string,
    class:string
}

var data = ref({
    key : 0,
    products : [] as Product[],
    empty : [] as number[]
});

onMounted(() => {
    data.value.products.push({ photo : '/images/product/woose.png', name : 'Woose', position : 'Development Library', url : 'https://www.nuget.org/packages/Woose.Data', class : "" });
    data.value.products.push({ photo : '/images/product/merrytoktok.png', name : 'Merrytoktok', position : 'Community Site', url : 'https://www.merrytoktok.com', class : "whiteline" });
    data.value.key += 1;

    data.value.empty.push(1);
    data.value.empty.push(2);
});
</script>