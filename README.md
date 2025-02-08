This is a attempt do create a [PostgREST](https://github.com/PostgREST/postgrest) version in c#.
This is not a version to compete with PostgREST and is not a translation from haskell to c#. 

It is a total new implementation just keeping the idea of PostgREST because I don't
know anything about haskell language, but I want to make a similar version.

This is an aspnet core app that runs with a posgres database server and receive rest calls with any json and 
insert, update or delete in the specified table in a more relaxed and lightweight way than the Microsoft Entity Framework.

You only need to create the table in the database, and you are ready to go.

Needless to say that this is a hobby project and is not production ready. 
This is a work in progress.


This is a hobby project the roadmap has no due dates yet but this is a broad roadmap to follow:

###  Roadmap:
    [ ] Create an example database
  - [ ] Implement POST (insert) method
    - [ ] Refine datatype validation  
    - [ ] Refine error handling
    - [ ] Implement unit tests for all phases
  - [ ] Implement PUT (update) method
    - [ ] Refine error handling    
  - [ ] Implement DELETE (delete) method
  - [ ] Implement GET (insert) method with id filter
  - [ ] Implement GET (insert) method with complex filters like odata filters
  - [ ] Jwt token authentication
  - [ ] Roles and permissions
  - [ ] Implement nested objects  
  - [ ] Implement other databases
  - [ ] Implement external cache for relationships and table metadata
  - [ ] Multitenency support
  

