import { createStore } from 'vuex';

export default createStore({
  state: {
    accessToken: '', // 인증토큰
    refreshToken: '', // 갱신토큰
    userinfo:{}
  },
  getters: {
    accessToken(state) {
      return state.accessToken;
    },
    refreshToken(state) {
      return state.refreshToken;
    },
    userinfo(state) {
      return state.userinfo;
    }
  },
  mutations: {
    setAccessToken(state, token) {
      state.accessToken = token;
    },
    setRefreshToken(state, token) {
      state.refreshToken = token;
    },
    clearTokens(state) {
      state.accessToken = '';
      state.refreshToken = '';
    },
    setUserInfo(state, user) {
      state.userinfo = user;
    },
    getUserInfo(state) {
      return state.userinfo;
    },
    clearUserInfo(state) {
      state.userinfo = {};
    }
  },
  actions: {
    saveTokens({ commit }, { accessToken, refreshToken }) {
      commit('setAccessToken', accessToken);
      commit('setRefreshToken', refreshToken);
    },
    saveUserInfo({ commit }, { userinfo }) {
      commit('setUserInfo', userinfo)
    }
  },
  modules: {

  },
});
