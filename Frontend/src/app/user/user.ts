
import { Component, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Api } from '../service/api';

@Component({
  selector: 'app-user',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './user.html',
  styleUrl: './user.css'
})
export class User {
  users: any[] = [];
  loading = false;
  isModalOpen = false;
  editUserForm: FormGroup;
  selectedUserId: string = '';

  constructor(
    private api: Api,
    private cd: ChangeDetectorRef,
    private fb: FormBuilder
  ) {
    // Initialize the form
    this.editUserForm = this.fb.group({
      fullName: ['', [Validators.required, Validators.minLength(2)]],
      email: ['', [Validators.required, Validators.email]]
    });
  }

  ngOnInit(): void {
    this.fetchUsers();
  }

  fetchUsers() {
    this.loading = true;
    this.api.getAllUsers().subscribe({
      next: (res) => {
        if (res.success) {
          this.users = res.users;
          this.cd.detectChanges();
        }
        this.loading = false;
      },
      error: (err) => {
        console.error('Error fetching users:', err);
        this.loading = false;
      }
    });
  }

  editUser(user: any) {
    this.selectedUserId = user.id;
    this.editUserForm.patchValue({
      fullName: user.fullName,
      email: user.email
    });
    this.isModalOpen = true;
  }

  closeModal() {
    this.isModalOpen = false;
    this.editUserForm.reset();
    this.selectedUserId = '';
  }

  saveUser() {
    if (this.editUserForm.valid) {
      const updatedUser = {
        id: this.selectedUserId,
        ...this.editUserForm.value
      };
      this.api.updateUser(this.selectedUserId, updatedUser).subscribe({
        next: (res) => { 
           console.log("response of updated data =>",res);
          if (res.success) {
            console.log('User updated successfully');
            alert('User updated successfully');
            this.fetchUsers(); 
            this.closeModal();
          }else{
             this.closeModal();
             alert(res.message);
          }
        },
        error: (err) => {
          console.error('Error updating user:', err);
        }
      });
    } else {
      Object.keys(this.editUserForm.controls).forEach(key => {
        this.editUserForm.get(key)?.markAsTouched();
      });
    }
  }

  deleteUser(id: string) {
    if (confirm('Are you sure you want to delete this user?')) {
      this.api.deleteUser(id).subscribe({
        next: (res) => {
          if (res.success) {
            console.log('User deleted successfully');
            alert('User deleted successfully');
            this.fetchUsers();
          }else{
             alert(res.message);
          }
        },
        error: (err) => {
          console.error('Error deleting user:', err);
        }
      });
    }
  }

  hasError(fieldName: string, errorType: string): boolean {
    const field = this.editUserForm.get(fieldName);
    return !!(field && field.hasError(errorType) && field.touched);
  }
}