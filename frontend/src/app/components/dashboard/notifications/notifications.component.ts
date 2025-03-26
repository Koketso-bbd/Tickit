import { Component, OnInit, OnInit } from '@angular/core';
import { DatePipe, NgFor, CommonModule } from '@angular/common';
import { NotificationsService } from '../../../notifications.service';
import { DatePipe, NgFor, CommonModule } from '@angular/common';
import { NotificationsService } from '../../../services/notifications.service';

@Component({
  selector: 'app-notifications',
  imports: [NgFor, DatePipe, CommonModule],
  imports: [NgFor, DatePipe, CommonModule],
  templateUrl: './notifications.component.html',
  styleUrls: ['./notifications.component.css'],
  styleUrls: ['./notifications.component.css'],
})
export class NotificationsComponent implements OnInit {
  notifications: any[] = [];

  constructor(private notificationService: NotificationsService) {}

  ngOnInit(): void {
    this.getNotifications();
  }

  getNotifications(): void {
    this.notificationService
      .getNotifications()
      .subscribe((notifications) => (this.notifications = notifications));
  }

  toggleReadStatus(notification: any): void {
    notification.isRead = !notification.isRead;
  }
}

export class NotificationsComponent implements OnInit {
  notifications: any[] = [];

  constructor(private notificationService: NotificationsService) {}

  ngOnInit(): void {
    this.getNotifications();
  }

  getNotifications(): void {
    this.notificationService
      .getNotifications()
      .subscribe((notifications) => (this.notifications = notifications));
  }

  toggleReadStatus(notification: any): void {
    notification.isRead = !notification.isRead;
  }
}