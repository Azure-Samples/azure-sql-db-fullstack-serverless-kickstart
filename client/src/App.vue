<template>
  <ToDoList />
</template>

<script>
import ToDoList from './components/ToDoList.vue'

export default {
  name: 'App',
  components: {
    ToDoList
  }
}
</script>

<style>
  @import url("https://unpkg.com/todomvc-app-css@2.2.0/index.css");
  @import url("./styles/todo-custom.css");
</style>