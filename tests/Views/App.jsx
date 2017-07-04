import TodoList from "./TodoList";
import NewTodoForm from "./NewTodoForm";
//import MuiThemeProvider from 'material-ui/styles/MuiThemeProvider';
//import AppBar from 'material-ui/AppBar';

import './Styles/App.css';

export default class App extends React.Component {
    /**
     * @type {AppProps}
     */
    props;
    constructor(props) {
        super(props);
        TodoActions.getAllTodos();
    }
    render() {
        return <div>
            <NewTodoForm></NewTodoForm>
            <TodoList {...this.props}></TodoList>
        </div>
    }
}