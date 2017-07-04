export default class TodoItem extends React.Component {
    render() {
        /** @type {Todo} */
        var todo = this.props.todo;
        console.log(todo);
        var style = {};
        if (todo.IsComplete) {
            style = Object.assign(style, { textDecoration: "line-through" });
        }
        return <li key={todo.Id} style={style}>
            {todo.Description} <button onClick={() => TodoActions.update({ completed: true, todoId: todo.Id })}> - </button>
        </li>
    }
}