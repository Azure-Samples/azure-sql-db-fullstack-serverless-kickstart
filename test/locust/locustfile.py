import time
from locust import HttpUser, task, between
from faker import Faker

fake = Faker()
Faker.seed(0)

class ToDoUser(HttpUser):
    wait_time = between(0.5, 1)

    @task(90)
    def get_by_id(self):
        id = fake.random_int(1, 2)
        self.client.get(f"/api/todo", name='/todo')

    @task(10)
    def post(self):        
        payload = {
            "title": fake.lexify("?" * fake.random_int(3, 7))
        }
        self.client.post(f"/api/todo", name='/todo', json=payload)
