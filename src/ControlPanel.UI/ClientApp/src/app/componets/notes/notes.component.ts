import { Component, OnInit } from '@angular/core';
import { AuthService } from 'src/app/services/auth/auth.service';
import {Note} from "../../helpers/Note";


@Component({
  selector: 'app-notes',
  templateUrl: './notes.component.html',
  styleUrls: ['./notes.component.css']
})
export class NotesComponent implements OnInit {
  notes: Note[] = [];
  selectedNote: Note | null = null;
  newNoteTitle: string = '';
  newNoteContent: string = '';

  constructor(private auth: AuthService) { }

  ngOnInit(): void {
    this.loadNotes();
  }

  loadNotes(): void {
    const userId = localStorage.getItem('UserId');
    const savedNotes = localStorage.getItem(`Notes_${userId}`);
    if (savedNotes) {
      this.notes = JSON.parse(savedNotes);
    }
  }

  saveNotes(): void {
    const userId = localStorage.getItem('UserId');
    if (userId && this.notes) {
      localStorage.setItem(`Notes_${userId}`, JSON.stringify(this.notes));
    }
  }

  selectNote(note: Note): void {
    // Если выбранная заметка совпадает с уже выбранной, установите selectedNote в null
    this.selectedNote = this.selectedNote === note ? null : note;
  }

  saveNote(): void {
    if (this.selectedNote) {
      if (!this.selectedNote.id) {
        this.selectedNote.id = new Date().toISOString();
        this.notes.push(this.selectedNote);
      }
      this.saveNotes();
    }
  }

  saveNewNote(): void {
    if (this.newNoteTitle && this.newNoteContent) {
      const newNote: Note = {
        id: new Date().toISOString(),
        title: this.newNoteTitle,
        content: this.newNoteContent
      };
      this.notes.push(newNote);
      this.saveNotes();
      this.newNoteTitle = '';
      this.newNoteContent = '';
    }
  }

  deleteNote(note: Note): void {
    const index = this.notes.indexOf(note);
    if (index !== -1) {
      this.notes.splice(index, 1);
      this.saveNotes();
    }
  }

  getBotLink(): string {
    const userId = localStorage.getItem('UserId') || '';
    const startParameter = encodeURIComponent(userId);
    return `https://t.me/ControlPanelServiceBot?start=${startParameter}`;
  }

  logout() {
    this.auth.signOut();
  }
}
