<template>
<section class="section" id="contact">
    <div class="container">
        <div class="row">
            <div class="col-lg-6">
                <h2 class="fw-bold mb-4">{{ t("content.contact.title") }}</h2>
                <p class="text-muted mb-5">{{ t("content.contact.bio") }}</p>
                
                <div>
                    <form method="post" @submit.prevent="fnSave">
                        <p id="error-msg">{{ errors.name }}</p>
                        <div id="simple-msg"></div>
                        <div class="row">
                            <div class="col-lg-6">
                                <div class="mb-4">
                                    <label for="name" class="text-muted form-label">{{ t("content.contact.forms.name") }}</label>
                                    <input
                                        type="text"
                                        class="form-control"
                                        id="name"
                                        placeholder="Required name"
                                        maxlength="30"
                                        v-model="partnership.name"
                                        required
                                    />
                                </div>
                            </div>
                            <div class="col-lg-6">
                                <div class="mb-4">
                                    <label for="email" class="text-muted form-label">{{ t("content.contact.forms.email") }}</label>
                                    <input
                                        type="email"
                                        class="form-control"
                                        id="email"
                                        placeholder="Required email"
                                        maxlength="255"
                                        v-model="partnership.email"
                                        required
                                    />
                                </div>
                            </div>
                            <div class="col-lg-6">
                                <div class="mb-4">
                                    <label for="company" class="text-muted form-label">{{ t("content.contact.forms.company") }}</label>
                                    <input
                                        type="text"
                                        class="form-control"
                                        id="company"
                                        placeholder="Required company"
                                        maxlength="80"
                                        v-model="partnership.company"
                                        required
                                    />
                                </div>
                            </div>
                            <div class="col-lg-6">
                                <div class="mb-4">
                                    <label for="phone" class="text-muted form-label">{{ t("content.contact.forms.phone") }}</label>
                                    <input
                                        type="text"
                                        class="form-control"
                                        id="phone"
                                        placeholder="Enter phone"
                                        maxlength="30"
                                        v-model="partnership.phone"
                                        required
                                    />
                                </div>
                            </div>
                            <div class="col-md-12">
                                <div class="mb-4">
                                    <label for="subject" class="text-muted form-label">{{ t("content.contact.forms.subject") }}</label>
                                    <input
                                        type="text"
                                        id="subject"
                                        class="form-control"
                                        placeholder="required subject"
                                        maxlength="30"
                                        v-model="partnership.title"
                                    />
                                </div>

                                <div class="mb-4 pb-2">
                                    <label for="comments" class="text-muted form-label">{{ t("content.contact.forms.content") }}</label>
                                    <textarea v-model="partnership.content" name="comments" id="comments" rows="4" class="form-control" placeholder="Enter message..." required></textarea>
                                </div>

                                <button type="submit" id="submit" name="send" class="btn btn-primary">{{ t("content.contact.forms.button") }}</button>
                            </div>
                        </div>
                    </form>
                </div>
                
            </div>
            <!-- end col -->

            <div class="col-lg-5 ms-lg-auto">
                <div class="mt-5 mt-lg-0">
                    <img src="/images/contact.png" alt="" class="img-fluid d-block" />
                    <p class="text-muted mt-5 mb-3"><i class="fa-solid fa-envelope"></i> {{ config.info.email }}</p>
                    <p class="text-muted mb-3"><i class="fa-brands fa-github"></i> https://github.com/roslyndev</p>
                    <ul class="list-inline pt-4">
                        <li class="list-inline-item me-3">
                            <a href="https://www.facebook.com/rosylndev" target="_blank" class="social-icon icon-mono avatar-xs rounded-circle"><i class="fa-brands fa-facebook"></i></a>
                        </li>
                        <li class="list-inline-item me-3">
                            <a href="https://www.youtube.com/@EarningAlone" target="_blank" class="social-icon icon-mono avatar-xs rounded-circle"><i class="fa-brands fa-youtube"></i></a>
                        </li>
                        <li class="list-inline-item">
                            <a href="https://roslyndev.tistory.com" target="_blank" class="social-icon icon-mono avatar-xs rounded-circle"><i class="fa-solid fa-blog"></i></a>
                        </li>
                    </ul>
                </div>
            </div>
        </div>
    </div>
</section>
</template>

<script setup lang="ts">
import { ApiHelper, DbMsg, MessageBox } from '@/models';
import { reactive, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import config from '@/Config';

const { t, locale } = useI18n();

interface Partnership {
	name:string
,	company:string
,	phone:string
,	email:string
,	title:string
,	content:string
}

var partnership = ref({} as Partnership);
var errors = reactive({
	name:''
});

const validateForm = () => {
	errors.name = partnership.value.name ? '' : 'Requird Name'
};

const isFormValid = () => {
	return !Object.values(errors).some((error) => error !== '');
};

const fnSave = async () => {
    validateForm();
	if (isFormValid()) {
        let jsonData:Partnership = Object.assign({}, partnership.value);
        let rst:any = await ApiHelper.Post("https://api.merrytoktok.com/api/Customer/Partnership/Save", jsonData);
        if (rst.check) {
            MessageBox.Success(t(DbMsg('save')), () => {
                document.location.reload();
            });
        } else {
            MessageBox.Alert(t(DbMsg(rst.message)));
        }
    }
}
</script>