import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './componets/login/login.component';
import { SignupComponent } from './componets/signup/signup.component';
import { DashboardComponent } from './componets/dashboard/dashboard.component';
import { AuthGuard } from './guards/auth.guard';
import {NotificationSettingsComponent} from "./componets/notification-settings/notification-settings.component";
import {NotesComponent} from "./componets/notes/notes.component";


const routes: Routes = [
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  {path:'login',component: LoginComponent},
  {path:'signup',component:SignupComponent},
  {path:'dashboard',component:DashboardComponent,canActivate:[AuthGuard]},
  { path: 'notification-settings', component: NotificationSettingsComponent,canActivate:[AuthGuard]  },
  { path: 'notes', component: NotesComponent,canActivate:[AuthGuard]  },


];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
