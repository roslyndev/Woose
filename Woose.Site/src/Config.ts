const Config = {
    mode : process.env.VUE_APP_ENV,
    api : process.env.VUE_APP_API_URL,
    isMobile : (window.innerWidth < 1024),
    info : {
        email : "yunlang97@naver.com"
    },
    origin : process.env.VUE_APP_BASE_URL,
    width:1280
};

export default Config;