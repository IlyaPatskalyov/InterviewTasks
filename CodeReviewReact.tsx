import * as React from "react";

export interface IProps { page?: string }

export interface IUserModel {
    id: string;
    name: string;
    profileLink: string;
}

export interface IState {
    count?: number;
    whoLiked?: IUserModel[]
}

export interface ILikeModel {
    count: number;
    whoLiked: IUserModel[];
}

export class Class3 extends React.Component<IProps, IState> {

    constructor(props: IProps, context: any) {
        props = Object.assign({ page: window.location.href }, props);

        super(props, context);
        this.state = { };
    }

    shouldComponentUpdate(nextProps : IProps, nextState : IState) : boolean {
        return nextProps.page != this.props.page;
    }

    async componentDidMount() : Promise<void> {
        try {
            let r = await fetch("/like/?page=" + this.props.page);
            let result = r.json() as ILikeModel;
            this.setState({count: result.count, whoLiked: result.whoLiked })
        }
        catch (e) {
            console.log(e);
        }
    }



    processClick() : void {
        fetch("/like/?page=" + this.props.page)
            .then(userInfo => this.setState({count: (this.state.count || 0) + 1}))
            .then(userInfo => this.state.whoLiked.push(userInfo), e => console.log(e) )
    }

    private renderUsers(users: IUserModel[]) {
        var result = {};
        for(var i = 0; i < users.length; i++) {
            var userInfo = users[i];
            result[userInfo] = <a onClick={() => window.location.href = userInfo.profileLink}>userInfo.name<br/></a>;
            if (usersCount++ > 10)
                break;
        }
        return Object.values(result);
    }

    render()  {
        if (!this.state.count) {
            this.setState({count: 0});
            return null;
        }

        return <div>
          <div onClick={this.processClick} >
                Like {!this.state.count ? "" : this.state.count}
                  </div>
            {renderUsers(this.state.whoLiked)}
        </div>;
    }
}
