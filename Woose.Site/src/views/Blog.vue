<template>
<section class="section" id="blog" v-if="blogposts !== null && blogposts !== undefined && blogposts.length > 0">
    <div class="container">
        <div class="row justify-content-center mb-4">
            <div class="col-lg-7 text-center">
                <h2 class="fw-bold">{{ t("content.blog.title") }}</h2>
                <p class="text-muted">{{ t("content.blog.bio") }}</p>
            </div>
        </div>
        <div class="row">
            <div class="col-lg-4" v-for="(item, index) in blogposts">
                <div class="card mt-4 border-0 shadow">
                    <div class="card-body p-4">
                        <span class="badge badge-soft-primary">{{ item.tag }}</span>
                        <h4 class="font-size-22 my-4"><a href="javascript: void(0);">{{ item.title }}</a></h4>
                        <p class="text-muted">{{ item.description }}</p>
                        <div class="d-flex align-items-center mt-4 pt-2">
                            <img src="/images/logo/roslyn.png" class="rounded-circle avatar-sm me-3" :alt="item.title" />
                            <div class="flex-body">
                                <h5 class="font-size-17 mb-0">Roslyn</h5>
                                <p class="text-muted mb-0 font-size-14">Developer, Korea</p>
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

const blogposts = [
    { 
        url : "https://roslyndev.tistory.com/10", 
        tag :"개발이야기", 
        title : "웹브라우저 console.log 의 맹점", 
        description : "프론트엔드를 개발하는 사람이라면 웹브라우저에 찍히는 console.log를 활용하는 경우는 종종 있을 것이다. 하지만, console.log를 사용함에 있어 주의해야할 것이..",
        thumbnail : "https://i1.daumcdn.net/thumb/S160x108/?scode=mtistory2&fname=https://blog.kakaocdn.net/dn/9tdCI/btsoxXeGjuk/BjT9U2kyyCDu7ofz6gq3F1/img.png" 
    },
    { 
        url : "https://roslyndev.tistory.com/9", 
        tag :"개발이야기", 
        title : "2023년도 스택오버플로우 설문조사",
        description : `모든 통계는 "전문 개발자" 기준입니다. (1) 사용언어 순위 역시 1위는 javascript 네요. Typescript까지 포함시키면 점유율은 압도적인 것 같습니다. 글로벌 순위다..`,
        thumbnail : "https://i1.daumcdn.net/thumb/S160x108/?scode=mtistory2&fname=https://blog.kakaocdn.net/dn/cBS5Ji/btsj6Peou7K/Pkvn0SH1zCRchsCp6BYxS1/img.png" 
    },
    { 
        url : "https://roslyndev.tistory.com/3", 
        tag :"개발이야기", 
        title : "AI가 대신하는 세상이 온다고?",
        description : "AI가 개발자를 대신한다. AI가 그림그리는 사람들을 대신한다. 물론 아주 틀린 말은 아니겠지만, 이렇게만 맹신한다면 어리석다. AI는 단순한 개발을 하는 코더, 단순..",
        thumbnail : "https://i1.daumcdn.net/thumb/S160x108/?scode=mtistory2&fname=https://blog.kakaocdn.net/dn/bDDeth/btrZKwMzLC5/cjRMyoBstFNm0JRRmihQbK/img.jpg" 
    }
];

</script>
