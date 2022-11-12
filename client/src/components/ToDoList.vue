<template>
  <section class="todoapp">
		<header class="header">
			<h1>todos</h1>			
			<input class="new-todo" autofocus autocomplete="off" placeholder="What needs to be done?" v-model="newTodo" @keyup.enter="addTodo" />
		</header>
    <section class="main" v-show="todos.length" v-cloak>			    
			<ul class="todo-list">
				<li v-for="todo in filteredTodos" class="todo" :key="todo.id" :class="{ completed: todo.completed, editing: todo == editedTodo }">
					<div class="view">
						<input @change="completeTodo(todo)" class="toggle" type="checkbox" v-model="todo.completed" />
						<label @dblclick="editTodo(todo)">{{ todo.title }}</label>
						<button class="destroy" @click="removeTodo(todo)"></button>
					</div>
					<input class="edit" type="text" v-model="todo.title" v-todo-focus="todo == editedTodo" @blur="doneEdit(todo)" @keyup.enter="doneEdit(todo)" @keyup.esc="cancelEdit(todo)" />
				</li>
			</ul>
		</section>
    <footer class="footer" v-show="todos.length" v-cloak>
			<span class="todo-count">
				<strong>{{ activeTodos.length }}</strong> {{ pluralize('item', activeTodos.length) }} left
			</span>
			<ul class="filters">
				<li>
					<a href="#/all" @click="visibility = 'all'" :class="{ selected: visibility == 'all' }">All</a>
				</li>
				<li>
					<a href="#/active" @click="visibility = 'active'" :class="{ selected: visibility == 'active' }">Active</a>
				</li>
				<li>
					<a href="#/completed" @click="visibility = 'completed'" :class="{ selected: visibility == 'completed' }">Completed</a>
				</li>
			</ul>
			<button class="clear-completed" @click="removeCompleted" v-show="completedTodos.length > 0">
				Clear completed
			</button>
		</footer>
  </section> 
  <footer class="info">
    <label v-if="!userDetails" id="login">[<a href=".auth/login/github">login</a>]</label>
		<label v-if="userDetails" id="logoff">[<a href=".auth/logout">logoff {{userDetails}}</a>]</label>    
		<p>Double-click to edit a todo</p>
		<p>Original <a href="https://github.com/vuejs/vuejs.org/tree/master/src/v2/examples/vue-20-todomvc">Vue.JS Sample</a> by <a href="http://evanyou.me">Evan You</a></p>		
		<p>Part of <a href="http://todomvc.com">TodoMVC</a></p>        
	</footer> 
</template>

<script lang="js">

var API = "/api/ef/todo";
var HEADERS = { 'Accept': 'application/json', 'Content-Type': 'application/json' };		

var filters = {
  all: function (todos) {
    return todos;
  },
  active: function (todos) {
    return todos.filter(function (todo) {
      return !todo.completed;
    });
  },
  completed: function (todos) {
    return todos.filter(function (todo) {
      return todo.completed;
    });
  }
};

export default {  
  data() {
    return {
      todos: [],
      newTodo: "",
			editedTodo: null,
			visibility: "all",
      userDetails: null
    };
  },
  
  mounted() {
    var visibility = window.location.hash.replace(/#\/?/, "");
    if (filters[visibility]) {
      this.visibility = visibility;
    } else {
      window.location.hash = "";
      this.visibility = "all";
    }

    fetch('/.auth/me')
    .then(res => {
      return res.json()
    })
    .then(payload => {
      const { clientPrincipal } = payload;
			this.userDetails = clientPrincipal?.userDetails;				
    });						

    fetch(API, { 
      headers: HEADERS, 
      method: "GET"      
    }).then(res => {
      return res.json();
    }).then(res => {				
      this.todos = res == null ? [] : res;
    });
  },
  
  computed: {
    activeTodos: function () { return filters["active"](this.todos) },

    completedTodos: function () { return filters["completed"](this.todos) },
    
    filteredTodos: function () { return filters[this.visibility](this.todos); },    
  },

  methods: {
    addTodo: function () {
      var value = this.newTodo && this.newTodo.trim();
      if (!value) return;

      fetch(API, {
        headers: HEADERS, 
        method: "POST", 
        body: JSON.stringify( { title: value } )
      }).then(res => {					
        if (res.ok) {												
          this.newTodo = ''
          return res.json();
        }
      }).then(res => {						
        this.todos.push(res[0]);
      })
    },

    completeTodo: function(todo) {      
      fetch(API + `/${todo.id}`, {
        headers: HEADERS, 
        method: "PATCH",
        body: JSON.stringify({completed: todo.completed})        
      });
    },    

    removeTodo: function (todo) {					      
      fetch(API + `/${todo.id}`, {
        headers: HEADERS, 
        method: "DELETE"        
      }).then(res => {
        if (res.ok) {
          var index = this.todos.indexOf(todo);
          this.todos.splice(index, 1);
        }
      })				
    },

    editTodo: function (todo) {
      this.beforeEditCache = todo.title;
      this.editedTodo = todo;
    },

    doneEdit: function (todo) {
      if (!this.editedTodo) {
        return;
      }
      this.editedTodo = null;
      todo.title = todo.title.trim();
      if (!todo.title) {
        this.removeTodo(todo);
      } else {
        fetch(API + `/${todo.id}`, {
          headers: HEADERS, 
          method: "PATCH", 
          body: JSON.stringify( { title: todo.title } )
        });						
      }
    },

    cancelEdit: function (todo) {
      this.editedTodo = null;
      todo.title = this.beforeEditCache;
    },

    removeCompleted: function () {
      filters.completed(this.todos).forEach(t => {						
        this.removeTodo(t);
      });					
    },

    pluralize: function(term, count) {
      if (count > 1) 
        return term + 's';
      else
        return term;
    }    
  },

  directives: {
    "todo-focus": function (el, binding) {
      if (binding.value) {
        el.focus();
      }
    }
  }  
};

</script>

