<template>
  <footer class="info">
    <label v-if="!userDetails" id="login">[<a href=".auth/login/github">login</a>]</label>
    <label v-if="userDetails" id="logoff">[<a href=".auth/logout">logoff {{ userDetails }}</a>]</label>
    <p>Double-click to edit a todo</p>
    <p>Original <a href="https://github.com/vuejs/vuejs.org/tree/master/src/v2/examples/vue-20-todomvc">Vue.JS
        Sample</a> by <a href="http://evanyou.me">Evan You</a></p>
    <p>Part of <a href="http://todomvc.com">TodoMVC</a></p>
  </footer>
</template>


<script lang="js">
export default {
  data() {
    return {
      userDetails: null
    };
  },

  mounted() {
    fetch('/.auth/me')
      .then(res => {
        return res.json()
      })
      .then(payload => {
        const { clientPrincipal } = payload;
        this.userDetails = clientPrincipal?.userDetails;
      });
  }
}
</script>