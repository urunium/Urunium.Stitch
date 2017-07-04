import TodoItem from './TodoItem';
export default class TodoList extends React.Component {
    /**
     * @type {AppProps}
     */
    props;
    /**
     *
     * @param {AppProps} props
     */
    constructor(props) {
        super(props);
    }
    render() {
        return <section id="main">
            <ul id="todo-list">
                {
                    this.props.currentState.Todos.map(todo => {
                        return <TodoItem todo={todo} />
                    })
                }
            </ul>
        </section>;
    }
}