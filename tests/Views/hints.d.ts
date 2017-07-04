declare interface BaseProps<TState> {
    currentState: TState;
    dispatcher: { dispatch: (action: string, payload: any) => {} };
}

declare interface AppProps extends BaseProps<AppState> {
}

declare interface AppState {
    Todos: Todo[];
}

declare interface Todo {
    Id: number;
    IsComplete: boolean;
    Description: string;
}

declare interface CreateTodoAction {
    description: string;
}

declare interface UpdateTodoAction {
    completed: boolean;
    todoId: number;
}

declare class TodoActions {
    static add(payload: CreateTodoAction);
    static update(payload: UpdateTodoAction);
    static getAllTodos();
}
