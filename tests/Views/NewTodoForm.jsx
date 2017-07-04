export default class NewTodoForm extends React.Component {
    render() {
        return <div>
            <label htmlFor="newtodo">Enter Todo Item: </label>
            <input id="newtodo" ref="newtodo" type="text" />
            <button onClick={() => { TodoActions.add({ description: this.refs['newtodo'].value }) }}>Add task</button>
        </div>
    }
}