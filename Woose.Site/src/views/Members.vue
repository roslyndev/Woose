<template>
<section class="section bg-light" id="team" v-if="data.members.length > 0">
    <div class="container">
        <div class="row justify-content-center mb-4">
            <div class="col-lg-7 text-center">
                <h2 class="fw-bold">{{ t("content.member.title") }}</h2>
                <p class="text-muted">{{ t("content.member.bio") }}</p>
            </div>
        </div>
        <div class="row" :key="data.key">
            <div class="col-lg-3 col-sm-6" v-for="(member, index) in data.members">
                <div class="team-box mt-4 position-relative overflow-hidden rounded text-center shadow">
                    <div class="position-relative overflow-hidden">
                        <img :src="member.photo" alt="" class="img-fluid d-block mx-auto" />
                        <ul class="list-inline p-3 mb-0 team-social-item">
                            <li class="list-inline-item mx-3">
                                <a :href="member.sns.facebook" class="team-social-icon h-primary"><i class="icon-sm" data-feather="facebook"></i></a>
                            </li>
                            <li class="list-inline-item mx-3">
                                <a :href="member.sns.portfolio" class="team-social-icon h-info"><i class="icon-sm" data-feather="website"></i></a>
                            </li>
                            <li class="list-inline-item mx-3">
                                <a :href="member.sns.blog" class="team-social-icon h-danger"><i class="icon-sm" data-feather="blog"></i></a>
                            </li>
                        </ul>
                    </div>
                    <div class="p-4">
                        <h5 class="font-size-19 mb-1">{{ member.name }}</h5>
                        <p class="text-muted text-uppercase font-size-14 mb-0">{{ member.position }}</p>
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

interface MemberInfo {
    photo : string,
    name : string,
    position : string,
    sns : {
        facebook : string,
        portfolio : string,
        blog : string
    }
}

var data = ref({
    key : 0,
    members : [] as MemberInfo[]
});

onMounted(() => {
    data.value.members.push({ photo : '/images/team/1.jpg', name : 'Frances Thompson', position : t('content.job.developer'), sns : { facebook : '#', portfolio : '#', blog : '#' }});
    data.value.members.push({ photo : '/images/team/2.jpg', name : 'John Jones', position : t('content.job.planner'), sns : { facebook : '#', portfolio : '#', blog : '#' }});
    data.value.members.push({ photo : '/images/team/3.jpg', name : 'Della Hobbs', position : t('content.job.designer'), sns : { facebook : '#', portfolio : '#', blog : '#' }});
    data.value.members.push({ photo : '/images/team/4.jpg', name : 'Troy Jordon', position : t('content.job.developer'), sns : { facebook : '#', portfolio : '#', blog : '#' }});
    data.value.key += 1;
});
</script>